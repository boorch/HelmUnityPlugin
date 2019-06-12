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

const float kMaxSampleRate = 22050.0f;
const float kPI = 3.141592653589793f;
const double kPI_double = 3.141592653589793;

inline float FastClip(float x, float minval, float maxval) { return (fabsf(x - minval) - fabsf(x - maxval) + (minval + maxval)) * 0.5f; }
inline float FastMin(float a, float b) { return (a + b - fabsf(a - b)) * 0.5f; }
inline float FastMax(float a, float b) { return (a + b + fabsf(a - b)) * 0.5f; }
inline int FastFloor(float x) { return (int)floorf(x); } // TODO: Optimize

char* strnew(const char* src);

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
