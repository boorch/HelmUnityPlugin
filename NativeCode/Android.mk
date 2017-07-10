include $(CLEAR_VARS)

# override strip command to strip all symbols from output library; no need to ship with those..
cmd-strip = $(TOOLCHAIN_PREFIX)strip $1 

MOPO_DIR = helm/mopo/src
SYNTHESIS_DIR = helm/src/synthesis
HELM_COMMON_DIR = helm/src/common
QUEUE_DIR = helm/concurrentqueue

LOCAL_ARM_MODE  := arm
LOCAL_PATH      := $(NDK_PROJECT_PATH)
LOCAL_MODULE    := libAudioPluginHelm
LOCAL_CFLAGS    := -Werror -I $(MOPO_DIR) -I $(SYNTHESIS_DIR) -I $(HELM_COMMON_DIR) -I $(QUEUE_DIR) -O3 -fPIC -std=c++11 

MOPO_CPPS := $(wildcard $(MOPO_DIR)/*.cpp)
SYNTHESIS_CPPS := $(wildcard $(SYNTHESIS_DIR)/*.cpp)
LOCAL_CPPS := $(wildcard $(LOCAL_DIR)/*.cpp)

LOCAL_SRC_FILES := $(MOPO_CPPS) $(SYNTHESIS_CPPS) $(HELM_COMMON_DIR)/helm_common.cpp $(LOCAL_CPPS)
LOCAL_LDLIBS    := -llog

include $(BUILD_SHARED_LIBRARY)
