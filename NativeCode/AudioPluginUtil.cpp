#include "AudioPluginUtil.h"
#include <stdarg.h>

#define ENABLE_TESTS ((PLATFORM_WIN || PLATFORM_OSX) && 1)

char* strnew(const char* src) {
  int length = strlen(src);
  char* newstr = new char[length + 1];
  memset(newstr, 0, (length + 1) * sizeof(char));
  memcpy(newstr, src, sizeof(char) * length);
  return newstr;
}

char* tmpstr(int index, const char* fmtstr, ...)
{
    static char buf[4][1024];
    va_list args;
    va_start(args, fmtstr);
    vsprintf(buf[index], fmtstr, args);
    va_end(args);
    return buf[index];
}

template<typename T> void UnitySwap(T& a, T& b) { T t = a; a = b; b = t; }

Mutex::Mutex()
{
#if PLATFORM_WIN
#if PLATFORM_WINRT
    BOOL const result = InitializeCriticalSectionEx(&crit_sec, 0, CRITICAL_SECTION_NO_DEBUG_INFO);
    assert(FALSE != result);
#else
    InitializeCriticalSection(&crit_sec);
#endif
#else
    pthread_mutexattr_t attr;
    pthread_mutexattr_init(&attr);
    pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE);
    pthread_mutex_init(&mutex, &attr);
    pthread_mutexattr_destroy(&attr);
#endif
}

Mutex::~Mutex()
{
#if PLATFORM_WIN
    DeleteCriticalSection(&crit_sec);
#else
    pthread_mutex_destroy(&mutex);
#endif
}

bool Mutex::TryLock()
{
#if PLATFORM_WIN
    return TryEnterCriticalSection(&crit_sec) != 0;
#else
    return pthread_mutex_trylock(&mutex) == 0;
#endif
}

void Mutex::Lock()
{
#if PLATFORM_WIN
    EnterCriticalSection(&crit_sec);
#else
    pthread_mutex_lock(&mutex);
#endif
}

void Mutex::Unlock()
{
#if PLATFORM_WIN
    LeaveCriticalSection(&crit_sec);
#else
    pthread_mutex_unlock(&mutex);
#endif
}

void RegisterParameter(
    UnityAudioEffectDefinition& definition,
    const char* name,
    const char* unit,
    float minval,
    float maxval,
    float defaultval,
    float displayscale,
    float displayexponent,
    int enumvalue,
    const char* description
    )
{
    assert(defaultval >= minval);
    assert(defaultval <= maxval);
    strcpy_s(definition.paramdefs[enumvalue].name, name);
    strcpy_s(definition.paramdefs[enumvalue].unit, unit);
    definition.paramdefs[enumvalue].description = (description != NULL) ? strnew(description) : (name != NULL) ? strnew(name) : NULL;
    definition.paramdefs[enumvalue].defaultval = defaultval;
    definition.paramdefs[enumvalue].displayscale = displayscale;
    definition.paramdefs[enumvalue].displayexponent = displayexponent;
    definition.paramdefs[enumvalue].min = minval;
    definition.paramdefs[enumvalue].max = maxval;
    if (enumvalue >= (int)definition.numparameters)
        definition.numparameters = enumvalue + 1;
}

// Helper function to fill default values from the effect definition into the params array -- called by Create callbacks
void InitParametersFromDefinitions(
    InternalEffectDefinitionRegistrationCallback registereffectdefcallback,
    float* params
    )
{
    UnityAudioEffectDefinition definition;
    memset(&definition, 0, sizeof(definition));
    registereffectdefcallback(definition);
    for (UInt32 n = 0; n < definition.numparameters; n++)
    {
        params[n] = definition.paramdefs[n].defaultval;
        delete[] definition.paramdefs[n].description;
    }
    delete[] definition.paramdefs; // assumes that definition.paramdefs was allocated by registereffectdefcallback or is NULL
}

void DeclareEffect(
    UnityAudioEffectDefinition& definition,
    const char* name,
    UnityAudioEffect_CreateCallback createcallback,
    UnityAudioEffect_ReleaseCallback releasecallback,
    UnityAudioEffect_ProcessCallback processcallback,
    UnityAudioEffect_SetFloatParameterCallback setfloatparametercallback,
    UnityAudioEffect_GetFloatParameterCallback getfloatparametercallback,
    UnityAudioEffect_GetFloatBufferCallback getfloatbuffercallback,
    InternalEffectDefinitionRegistrationCallback registereffectdefcallback
    )
{
    memset(&definition, 0, sizeof(definition));
    strcpy_s(definition.name, name);
    definition.structsize = sizeof(UnityAudioEffectDefinition);
    definition.paramstructsize = sizeof(UnityAudioParameterDefinition);
    definition.apiversion = UNITY_AUDIO_PLUGIN_API_VERSION;
    definition.pluginversion = 0x010000;
    definition.create = createcallback;
    definition.release = releasecallback;
    definition.process = processcallback;
    definition.setfloatparameter = setfloatparametercallback;
    definition.getfloatparameter = getfloatparametercallback;
    definition.getfloatbuffer = getfloatbuffercallback;
    registereffectdefcallback(definition);
}

#define DECLARE_EFFECT(namestr, ns) \
    namespace ns \
    { \
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback            (UnityAudioEffectState* state); \
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback           (UnityAudioEffectState* state); \
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback           (UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels); \
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback (UnityAudioEffectState* state, int index, float value); \
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback (UnityAudioEffectState* state, int index, float* value, char *valuestr); \
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback    (UnityAudioEffectState* state, const char* name, float* buffer, int numsamples); \
    int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition); \
    }
#include "PluginList.h"
#undef DECLARE_EFFECT

#define DECLARE_EFFECT(namestr, ns) \
DeclareEffect( \
definition[numeffects++], \
namestr, \
ns::CreateCallback, \
ns::ReleaseCallback, \
ns::ProcessCallback, \
ns::SetFloatParameterCallback, \
ns::GetFloatParameterCallback, \
ns::GetFloatBufferCallback, \
ns::InternalRegisterEffectDefinition);

extern "C" UNITY_AUDIODSP_EXPORT_API int AUDIO_CALLING_CONVENTION UnityGetAudioEffectDefinitions(UnityAudioEffectDefinition*** definitionptr)
{
    static UnityAudioEffectDefinition definition[256];
    static UnityAudioEffectDefinition* definitionp[256];
    static int numeffects = 0;
    if (numeffects == 0)
    {
        #include "PluginList.h"
    }
    for (int n = 0; n < numeffects; n++)
        definitionp[n] = &definition[n];
    *definitionptr = definitionp;
    return numeffects;
}

// Simplistic unit-test framework
#if ENABLE_TESTS
    #define NAP_TESTSUITE(name) \
        namespace testsuite_##name { inline const char* GetSuiteName() { return #name; } }\
        namespace testsuite_##name
    #define NAP_UNITTEST(name) \
        struct NAP_Test_##name { NAP_Test_##name(const char* testname); };\
        static NAP_Test_##name test_##name(#name);\
        NAP_Test_##name::NAP_Test_##name(const char* testname)
    #define NAP_CHECK(...) \
        do\
        {\
            if(!(__VA_ARGS__))\
            {\
                printf("%s(%d): Unit test '%s' failed for expression '%s'.\n", __FILE__, __LINE__, testname, #__VA_ARGS__);\
                assert(false && "Unit test in native audio plugin framework failed!");\
            }\
        } while(false)
#else
    #define NAP_TESTSUITE(name) namespace testsuite_##name
    #define NAP_UNITTEST(name) static void test_##name()
    #define NAP_CHECK(...) do {} while(false)
#endif
