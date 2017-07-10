#!/bin/sh
$ANDROID_NDK_ROOT/ndk-build NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk $*
mkdir -p ../Assets/Helm/Plugins/Android/
rm -rf ../Assets/Helm/Plugins/Android/libs
mv libs ../Assets/Helm/Plugins/Android/libs

echo ""
echo "cleaning up libs"
rm -rf libs

echo "Done!"
