runtime="linux-arm64"
# 获取当前脚本所在目录
project_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

# 发布优化版本
echo "开始发布优化版本..."
if ! dotnet publish MerryBot/MerryBot.csproj -c Release \
    -r $runtime \
    --self-contained false \
    -p:PublishTrimmed=false \
    -p:TrimMode=link \
    -p:PublishSingleFile=false \
    -p:EnableCompressionInSingleFile=false \
    -p:PublishReadyToRun=true \
    -p:PublishReadyToRunShowWarnings=true \
    -p:PublishAot=false \
    -p:DebugType=None \
    -p:DebugSymbols=true; then
    echo "dotnet publish 失败，退出脚本"
    exit 1
fi

# 运行应用程序
echo "启动应用程序..."
cd MerryBot/bin/Release/net10.0/$runtime/publish
./MerryBot



