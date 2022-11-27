// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma once
#include <cstdint>
#include "../define.h"

using KeyInputCallback = PINVOKE_RETURN(void) (uint32_t vkCode, bool isDown);

extern "C" {
	ABI_RETURN(bool) HookKeyboard(KeyInputCallback onKeyInput);
	ABI_RETURN(void) UnhookKeyboard();
}
