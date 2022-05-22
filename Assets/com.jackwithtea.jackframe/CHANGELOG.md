# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),  
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).  

## [Develop]
## [1.8.0] - 2022-05-22
### Added
- 定点数Fixed64  

## [1.7.0] - 2022-05-21
### Changed
- 大量调整目录与程序集依赖关系  
- 现在Independence目录下的都是独立的  

## [1.6.1] - 2022-05-10
### Changed
- JackBuffer Add Using When Genrate  

## [1.6.0] - 2022-05-09
### Changed
- 移除无用类  
- 大改工程目录  

## [1.5.2] - 2022-05-09
### Added
- FrameUIManager 增加 GetByType 接口  
### Changed
- FrameUIManager 修改Get为GetByID  

## [1.5.1] - 2022-05-09
### Added
- FrameUIManager 增加 OpenByType 接口  
### Changed
- FrameUIManagerBase 去掉 Base  
- FrameUIManager 修改Open为OpenByID  

## [1.5.0] - 2022-05-09
### Added
- FrameUIManagerBase 改为挂载的形式  

## [1.4.1] - 2022-04-10
### Added
- MultiKeySortedDictionary  

## [1.4.0] - 2022-04-10
### Added
- Anymotion  

## [1.3.0] - 2022-04-10
### Added
- 对象池 Pool  

## [1.2.12] - 2022-04-06
### Changed
- GitRepoTool 新增默认package.json参数  

## [1.2.11] - 2022-03-27
### Changed
- FSMBase -> FSM, 移除泛型  

## [1.2.10] - 2022-03-26
### Added
- IntHelper::UIntCompress / Uncompress  

## [1.2.9] - 2022-03-23
### Changed
- FrameUIPanelBase 新增 virtual 修饰  

## [1.2.8] - 2022-03-22
### Added
- GitPublishTool 重新获取版本号  

## [1.2.7] - 2022-03-19
### Added
- GitPublishTool 打包 UnityPackage 的功能  

## [1.2.6] - 2022-03-18
### Fixed
- 修复 GitPublishTool 路径前加 / 导致识别出错  

## [1.2.5] - 2022-03-18
### Fixed
- 修复 GitPublishTool 无 VERSION 文件时报错  

## [1.2.4] - 2022-03-17
### Changed
- GitPublish 工具改为 EditorWindow 的操作方式  

## [1.2.3] - 2022-03-14
### Fixed
- 依赖NewtonSoft.Json的方式改为从UPM依赖  

## [1.2.2] - 2022-03-14
### Added
- 版本发布工具  

## [1.2.1] - 2022-03-11
### Changed
- JackBuffer: 支持 Varint 与 ZigZag  

## [1.2.0] - 2022-03-03
### Added
- NetworkWeaver.TCP: 网络通信集成  

## [1.1.0] - 2022-03-02
### Added
- JackAST: 集成 Roslyn 的代码生成工具  
- JackBuffer: 数据序列化与反序列化(二进制序列化协议), 自动生成代码, C# 友好  

## [1.0.0] - 2022-02-26
### Added
- JackFrame: C#标准库扩展 / C#标准类 / Unity标准库扩展 / Unity UI框架  
- JackEditorTool: 脏脚本编译工具 / CSProj清理与再生成工具 / 图片切割导出工具  

- [Develop]: https://github.com/chenwansal/JackFrame  
