// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#include "rawinput.h"

#include <windows.h>
#include <future>
#include <atomic>

#pragma comment(lib,"user32.lib") 

KeyInputCallback callback;

std::future<bool> threadKeyboard;
DWORD threadKeyboardID;

struct GuardedHWND {
	HWND hWnd;
	GuardedHWND(HWND hWnd = NULL) : hWnd(hWnd) {}
	inline operator HWND () { return hWnd; }
	~GuardedHWND() { if (hWnd) DestroyWindow(hWnd); }
};

LRESULT CALLBACK WindowProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
	switch (uMsg) {
		case WM_INPUT: {
			auto hr = (HRAWINPUT)lParam;

			UINT dataSize;
			GetRawInputData(hr, RID_INPUT, NULL, &dataSize, sizeof(RAWINPUTHEADER));
			if (dataSize == 0) return 0;

			auto data = std::make_unique<BYTE[]>(dataSize);
			if (GetRawInputData(hr, RID_INPUT, data.get(), &dataSize, sizeof(RAWINPUTHEADER)) == dataSize) {
				RAWINPUT* raw = (RAWINPUT*)data.get();
				if (raw->header.dwType == RIM_TYPEKEYBOARD) {
					RAWKEYBOARD& kb = raw->data.keyboard;
					//if ((kb.Flags & RI_KEY_MAKE) > 0 || (kb.Flags & RI_KEY_BREAK) > 0) {
						//https://stackoverflow.com/questions/5920301/distinguish-between-left-and-right-shift-keys-using-rawinput/71885051#71885051
						uint16_t vkCode = kb.VKey;
						uint16_t scanCode = kb.MakeCode;
						const bool keyHasE0Prefix = (kb.Flags & RI_KEY_E0) == RI_KEY_E0;
						const bool keyHasE1Prefix = (kb.Flags & RI_KEY_E1) == RI_KEY_E1;

						scanCode |= keyHasE0Prefix ? 0xe000 : 0;
						scanCode |= keyHasE1Prefix ? 0xe100 : 0;
						switch (vkCode){
							case VK_SHIFT:
							case VK_CONTROL:
							case VK_MENU:
								vkCode = LOWORD(MapVirtualKeyW(scanCode, MAPVK_VSC_TO_VK_EX));
								break;
						}
						callback(vkCode, (kb.Flags & RI_KEY_BREAK) == 0);
					//}
				}
			}

			return 0;
		}
	}
	return DefWindowProc(hWnd, uMsg, wParam, lParam);
}

ABI_RETURN(bool) HookKeyboard(KeyInputCallback onKeyInput) {
	if (onKeyInput == nullptr) return false;

	UnhookKeyboard();
	callback = onKeyInput;

	std::atomic_bool threadKeyboardStarted = false;

	threadKeyboardID = 0;
	threadKeyboard = std::async(std::launch::async, [&]() {
		THREAD_NAME("MaTech Win32 RawInput");

		static WNDCLASS wc = []() {
			WNDCLASS wc = {};
			wc.lpfnWndProc = WindowProc;
			wc.hInstance = GetModuleHandle(NULL);
			wc.lpszClassName = TEXT("RawInputWindow");
			return RegisterClass(&wc) ? wc : WNDCLASS{};
		}();

		if (wc.lpszClassName == NULL)
			return false;

		GuardedHWND window = CreateWindow(wc.lpszClassName, NULL, 0, 0, 0, 0, 0, NULL, NULL, GetModuleHandle(NULL), NULL);
		if (window == NULL)
			return false;

		RAWINPUTDEVICE rid = {};
		rid.usUsagePage = 0x01; // generic hid
		rid.usUsage = 0x06;  // 0x02 - mouse  0x06 - keyboard
		rid.dwFlags = RIDEV_INPUTSINK; // run in background
		rid.hwndTarget = window;

		if (!RegisterRawInputDevices(&rid, 1, sizeof(rid)))
			return false;

		threadKeyboardID = GetCurrentThreadId();
		threadKeyboardStarted = true;

		MSG msg;
		while (GetMessage(&msg, NULL, 0, 0)) {
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}

		return true;
	});

	while (!threadKeyboardStarted) {
		if (threadKeyboard.wait_for(std::chrono::milliseconds(0)) == std::future_status::ready)
			return false;
	}
	return true;
}

ABI_RETURN(void) UnhookKeyboard() {
	if (threadKeyboardID == 0) return;
	PostThreadMessage(threadKeyboardID, WM_QUIT, 0, 0);
	threadKeyboard.wait();
}
