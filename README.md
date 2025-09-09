# Shadalyze
Analyze the performance of glsl shader on Android by using Mali Offline Compiler, Package Developed on Unity 2022.3.

This plugin includes the Mali Offline Compiler copied from Arm Performance Studio 2024.6 for convenience. If there is any infringement, please contact me for removal.

## How to use
1. Assets -> Shadalyze -> Analyze Shader Performance ( For Shader, ShaderVariantCollection, Material assets)
2. Window -> Analysis -> Frame Debugger Shadalyze ( Analyze the compiled shader variant in editor according to shader name, pass name and keywords in Frame Debugger, not the real shader executed on remote device. )

## TODO
1. Analyze when Shader build processing, it requires an efficient shader caching way because the current shader compilation and analysis process takes a long time.
2. performance data analysis for amounts of shader variants ( require parser of analysis report )

## Reference
[Mali Offline Compiler](https://developer.arm.com/Tools%20and%20Software/Mali%20Offline%20Compiler)

[liamcary/UnityShaderAnalyzer](https://github.com/liamcary/UnityShaderAnalyzer)

[Delt06/malioc-unity](https://github.com/Delt06/malioc-unity)