// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma once

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64)
#   define PLATFORM_WIN 1
#   if defined(WINAPI_FAMILY) && (WINAPI_FAMILY == WINAPI_FAMILY_APP)
#       define PLATFORM_WINRT 1
#   endif
#elif defined(__MACH__) || defined(__APPLE__)
#   define PLATFORM_OSX 1
#   include <TargetConditionals.h>
#   if TARGET_OS_IPHONE
#       define PLATFORM_IOS 1
#   elif TARGET_OS_MAC
#       define PLATFORM_MAC 1
#   else
#       error "Unknown Apple platform"
#   endif
#elif defined(__ANDROID__)
#   define PLATFORM_ANDROID 1
#elif defined(__linux__)
#   define PLATFORM_LINUX 1
#endif

#if defined(_AMD64_) || defined(__LP64__) || defined(_M_ARM64)
#   define PLATFORM_ARCH_64 1
#   define PLATFORM_ARCH_32 0
#else
#   define PLATFORM_ARCH_64 0
#   define PLATFORM_ARCH_32 1
#endif

#ifdef PLATFORM_WIN
#define DLLEXPORT __declspec(dllexport)
#define STDCALL __stdcall
#else
#define DLLEXPORT __attribute__((visibility("default")))
#define STDCALL
#endif

#define ABI_RETURN(return_type) DLLEXPORT return_type STDCALL
#define PINVOKE_RETURN(return_type) return_type (STDCALL*) 

#ifdef PLATFORM_WIN
#define ASSERT_BREAK(condition) {if(!(condition))__debugbreak();}
#else
#define ASSERT_BREAK(condition) {if(!(condition))__breakpoint(42);}
#endif

#ifdef PLATFORM_WIN
#define NOMINMAX
#include <windows.h>
#include <processthreadsapi.h>
#define THREAD_NAME(s) SetThreadDescription(GetCurrentThread(), L##s)
#elif PLATFORM_ANDROID || PLATFORM_LINUX
#include <pthread.h>
#define THREAD_NAME(s) pthread_setname_np(pthread_self(), s)
#elif PLATFORM_OSX
#include <pthread.h>
#define THREAD_NAME(s) pthread_setname_np(s)
#else
#define THREAD_NAME(s) ((void)s)
#endif

#ifdef PLATFORM_WIN
#if !_ENABLE_EXTENDED_ALIGNED_STORAGE
#define _ENABLE_EXTENDED_ALIGNED_STORAGE 1
#endif
#endif
