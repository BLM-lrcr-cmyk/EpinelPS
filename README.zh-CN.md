# EpinelPS — 个人本地服实验版

<div align="center">

![个人 fork](https://img.shields.io/badge/%E4%B8%AA%E4%BA%BA%20fork-%E8%87%AA%E7%94%A8%E4%BF%AE%E5%A4%8D-7c3aed?style=flat-square)
[![上游项目](https://img.shields.io/badge/%E4%B8%8A%E6%B8%B8-EpinelPS%2FEpinelPS-2563eb?style=flat-square)](https://github.com/EpinelPS/EpinelPS)
[![许可证 AGPL-3.0](https://img.shields.io/badge/license-AGPL--3.0-green?style=flat-square)](LICENSE)
[![English README](https://img.shields.io/badge/README-English-blue?style=flat-square)](README.md)

**这是 EpinelPS 的个人 fork，用来保存自用修复和本地服调整。**

不追求花里胡哨地改资源，优先让服务端稳一点、数据库安全一点、等级别把客户端卡死。

</div>

---

## 这是什么？

EpinelPS 是一个用于 2D 动漫 RPG 游戏的 private/local server 项目。上游官方仓库在这里：

https://github.com/EpinelPS/EpinelPS

这个仓库是个人 fork，不是上游官方发布源。主要用于保存自用修复。

## 这个 fork 改了什么？

目前主要改动是角色等级和数据库稳定性：

- `SetLevel` 指令上限从 `999` 调整为 `1400`
- 启动或重载数据库时，角色等级低于 `1` 会修正为 `1`
- 角色等级超过 `1400` 会自动修正为 `1400`
- 同步器等级也限制在安全范围内
- 服务端返回给客户端的角色等级/同步器等级不会超过 `1400`
- `SynchroMaxLv` 返回值也限制到 `1400`
- `db.json` 保存改成更安全的写法：
  - 先写入临时文件
  - 再替换正式 `db.json`
  - 同时保留 `db.json.bak`
- 如果 `db.json` 变空或损坏，会尝试从 `db.json.bak` 恢复

> [!IMPORTANT]
> 当前客户端静态等级数据最高只到 `1400`。等级超过 `1400` 后，客户端会读取不到对应的等级/属性数据，常见表现就是进入游戏或打开角色相关界面时卡在加载界面、无限加载。这个 fork 不修改游戏资源，所以服务端会把等级限制在 `1400`。

## 怎么使用？

如果只是普通使用，流程和上游项目一样：

1. 下载上游官方构建：
   https://nightly.link/EpinelPS/EpinelPS/workflows/dotnet-desktop/main/Server%20and%20Server%20selector.zip
2. 关闭游戏和启动器
3. 以管理员身份运行 `ServerSelector.Desktop.exe`
4. 选择 `Local server`
5. 保存设置
6. 启动 `EpinelPS.exe`
7. 在启动器里注册账号，邮箱验证码随便填即可

管理后台：

https://127.0.0.1/admin/

第一个创建的账号会成为管理员账号。

> [!NOTE]
> 游戏更新前，建议先切回官方服务器，让游戏正常更新资源。

## 关于等级上限

这个 fork 的策略是：不碰游戏资源，只在服务端做安全限制。

超过 `1400` 会发生什么：

- 客户端静态表里没有 `1401` 及以上的等级数据
- 手动改成 `4500`、`9999` 或更高时，客户端会读取不到对应数据
- 常见结果是卡在加载界面，或者打开角色/同步器相关界面时无限加载
- 服务端启动或重载数据库时，会把超过 `1400` 的等级自动压回 `1400`
- `SetLevel` 指令也只能设置到 `1400`

这样做不是限制玩法，而是避免客户端因为读取不存在的数据而进不去。

## 上游项目

官方仓库：

https://github.com/EpinelPS/EpinelPS

Linux 启动器：

https://github.com/EpinelPS/EpinelPSLauncher

上游 TODO / Issues：

- https://github.com/orgs/EpinelPS/projects/1
- https://github.com/EpinelPS/EpinelPS/issues

## 许可证

本项目沿用上游许可证：[AGPL-3.0](LICENSE)。
