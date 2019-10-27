#pragma once

#include "AudioPluginInterface.h"

#include <math.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <assert.h>

#if PLATFORM_WIN
#   include <windows.h>
#else
#   include <pthread.h>
#   define strcpy_s strcpy
#endif

typedef int (*InternalEffectDefinitionRegistrationCallback)(UnityAudioEffectDefinition& desc);

namespace AudioHelm {
  char* strnew(const char* src);
  char* tmpstr(int index, const char* fmtstr, ...);

  class Mutex
  {
  public:
      Mutex();
      ~Mutex();
  public:
      bool TryLock();
      void Lock();
      void Unlock();
  protected:
  #if PLATFORM_WIN
      CRITICAL_SECTION crit_sec;
  #else
      pthread_mutex_t mutex;
  #endif
  };

  class MutexScopeLock
  {
  public:
      MutexScopeLock(Mutex& _mutex, bool condition = true) : mutex(condition ? &_mutex : NULL) { if (mutex != NULL) mutex->Lock(); }
      ~MutexScopeLock() { if (mutex != NULL) mutex->Unlock(); }
  protected:
      Mutex* mutex;
  };
}

void RegisterParameter(
    UnityAudioEffectDefinition& desc,
    const char* name,
    const char* unit,
    float minval,
    float maxval,
    float defaultval,
    float displayscale,
    float displayexponent,
    int enumvalue,
    const char* description = NULL
    );

void InitParametersFromDefinitions(
    InternalEffectDefinitionRegistrationCallback registereffectdefcallback,
    float* params
    );

void DeclareEffect(
    UnityAudioEffectDefinition& desc,
    const char* name,
    UnityAudioEffect_CreateCallback createcallback,
    UnityAudioEffect_ReleaseCallback releasecallback,
    UnityAudioEffect_ProcessCallback processcallback,
    UnityAudioEffect_SetFloatParameterCallback setfloatparametercallback,
    UnityAudioEffect_GetFloatParameterCallback getfloatparametercallback,
    UnityAudioEffect_GetFloatBufferCallback getfloatbuffercallback,
    InternalEffectDefinitionRegistrationCallback registereffectdefcallback
    );
