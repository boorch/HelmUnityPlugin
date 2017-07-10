#!/bin/sh
$ANDROID_NDK_ROOT/ndk-build NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk $*
mkdir -p ../Assets/Plugins/Android
mv libs/armeabi/libAudioPluginHelm.so ../Assets/Plugins/Android/

echo ""
echo "cleaning up libs"
rm -rf libs

echo "Done!"
