#pragma once
#include <cstdint>
#include "../define.h"

using KeyInputCallback = PINVOKE_RETURN(void) (uint32_t vkCode, bool isDown);

extern "C" {
	ABI_RETURN(bool) HookKeyboard(KeyInputCallback onKeyInput);
	ABI_RETURN(void) UnhookKeyboard();
}
