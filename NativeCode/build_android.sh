#!/bin/sh
$ANDROID_NDK_ROOT/ndk-build NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk $*
mkdir -p ../Assets/AudioHelm/Plugins/Android/
rm -rf ../Assets/AudioHelm/Plugins/Android/libs
mv libs ../Assets/AudioHelm/Plugins/Android/libs

echo ""
echo "cleaning up libs"
rm -rf libs

echo "Done!"
