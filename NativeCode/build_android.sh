#!/bin/sh
$ANDROID_NDK_ROOT/ndk-build NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk $*
mkdir -p ../Assets/AudioHelm/Plugins/Android/
rm -rf ../Assets/AudioHelm/Plugins/Android/libs
mv libs ../Assets/AudioHelm/Plugins/Android/libs
git checkout ../Assets/AudioHelm/Plugins/Android/libs/*.meta
git checkout ../Assets/AudioHelm/Plugins/Android/libs/armeabi-v7a/*.meta
git checkout ../Assets/AudioHelm/Plugins/Android/libs/x86/*.meta
git checkout ../Assets/AudioHelm/Plugins/Android/libs/arm64-v8a/*.meta
git checkout ../Assets/AudioHelm/Plugins/Android/libs/x86_64/*.meta

echo ""
echo "cleaning up libs"
rm -rf libs

echo "Done!"
