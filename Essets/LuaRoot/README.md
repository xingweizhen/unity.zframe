# 游戏框架Lua部分使用说明（Unity+xLua）

## 预置库
|库名|说明|实现|
|--|--|--|
|`libunity`|封装了Unity引擎的一些接口（不包含资源管理和界面）。|C#|
|`libasset`|封装资源管理相关的接口。|C#|
|`libugui`|封装了UGUI相关的接口（布局、设置文本、设置图片等）。|C#|
|`libsystem`|封装了一些设备系统相关的接口（杂项）。|C#|
|`libnetwork`|封装了网络相关接口（底层接口）。|C#|
|`libcsharpio`|封装一些对文件和文件夹进行操作的接口。|C#|
|`libui`|主要包含了一批通用的界面的管理接口。|Lua|
|`libdata`|封装动态数据管理的接口，也负责保存用户动态数据。|Lua|
|`libtimer`|一个定时器库，计时间隔为1秒。主要用于管理游戏中的各种倒计时事件。|Lua|
|`libscene`|封装了从Lua侧管理Unity场景的一些接口。|Lua|
|`libnet`|扩展了`libnetwork`库（高层接口），管理http连接和下载以及多tcp连接。|Lua|
|`ui`|管理界面窗口的打开、关闭和层级控制。|Lua|
|[`cjson`](http://regex.info/blog/lua/json "JSON")|一个纯lua实现的json库。|Lua|
|[`serpent`](https://github.com/pkulchenko/serpent "serpent")|一个纯lua实现的table转文本库。|Lua|

## 全局表
|表名|说明|例子|
|--|--|--|
|`_G.MT`|内部包含几个基本的元表：常量表(`Const`)，只读表(`ReadOnly`)，自动表(`AutoGen`)||
|`_G.UE`|引用了`CS.UnityEngine`，可以访问已导出的`UnityEngine`命名空间下的类||
|`_G.UGUI`|引用了`CS.ZFrame.UGUI`，可以访问已导出的`ZFrame.UGUI`命名空间下的类||
|`_G.System`|引用了`CS.System`，可以访问已导出的`System`命名空间下的类||
|`_G.PKG`|几乎所有已加载的自定义的lua文件都保存到这个表下。|`_G.PKG["game/event"]`|
|`_G.DEF`|通过该表来方便的访问一个“类”。需要把自定义的类放在目录`data/object`下面，且文件名全小写。|`_G.DEF.Hero.new()`|
|`_G.ENV`|存了一些环境变量（杂项）。||

## 全局函数
|函数名|说明|例子|
|--|--|--|
|`import(path,silent)`|加载一个文件，返回结果保存到`_G.PKG`。参数`silent`表示是否静默加载，静默时如果目标路径不存在则不会有错误警告。||
|`GO(root,path,com)`|该函数生成一个table，描述了一个Unity GameObject。传递到C#侧作为参数使用。使用该表可以减少推到lua环境的C#对象引用||
|`trycall(f, ...)`|封装了一个安全模式调用函数的方法，会捕获异常，并通过一个自定义的错误函数输出异常。||
|`newlog()`|产生一个临时管理日志的函数`func(fmt, ...)`，调用此函数会出现以下结果：当参数`fmt`存在时，使用其生成日志并记录到内部表；不存在时，将输出所有记录的日志。||
|`memoize(func)`|一个简单的记忆函数，记录函数`func`执行时返回的结果，`func`必须只有一个参数且直返回一个结果。该结果会永久记录（因为没用weak表...）。||
|`printf(fmt, ...)`|封装了`print`，以简化格式化输出时的代码。||
|`cfgname(Cfg)`|调试模式时返回`string.format("%s #%d", Cfg.name, Cfg.id)`，否则返回`Cfg.name`。方便调试时能看到一个配置的ID。||
|`next_action(key,action)`|标志某个方法`action`在下0.1秒后再执行。|`key`是一个Unity对象|
|`remove_action(key,action)`|取消被`next_action`标志的一个方法。|`key`是一个Unity对象|

## 全局变量
|变量名|类型|说明|
|--|--|--|
|lang|字符串|当前使用的本地化语言。|

## 预置的类
**以下代码均定义在`framework/util`下面**

|类名|说明|位置|
|--|--|--|
|`_G.DEF.Client`|客户端Tcp控制类|`framwork/util/client.lua`|
|`_G.DEF.Pref`|管理`UnityEngine.PlayerPrefs`的类|`framwork/util/pref.lua`|
|`_G.DEF.Link`|数据结构：链表|`framwork/util/link.lua`|
|`_G.DEF.Queue`|数据结构：队列|`framwork/util/queue.lua`|
|`_G.DEF.Stack`|数据结构：栈|`framwork/util/stack.lua`|
|`_G.DEF.Tree`|数据结构：树|`framwork/util/tree.lua`|

## 预置的界面
|表名|说明|例子|
|--|--|--|
|`libui.MBox`|通用对话框。|见`framework/ui/messagebox.lua`|
|`libui.Toast`|通用浮动信息|见`framework/ui/toast.lua`|
|`libui.MonoToast`|一个一个显示的浮动信息|见`framework/ui/monotoast.lua`|

# 开始自己的代码
### 添加自己的初始化代码
在`game/init.lua`中添加自己的初始化代码。
* 添加启动资源和启动初始化函数（必须）。
  `libscene.add_level(0, loadedCbf, assetsCbf)`
  
* 添加各个场景的**预加载资源**和**回调函数**（必须）。
  `libscene.add_level("levelName", loadedCbf, assetsCbf)`

```seq
title: 游戏启动流程
Note over csharp: 游戏启动，加载初始资源
csharp->lua: "framework/main.lua"
Note over lua: 环境初始化
lua->init: "game/init.lua"
Note over init: 游戏初始化
Note right of init: libscene.add_level()\n...
csharp->lua: libscene.on_level_loaded
Note over lua: 加载自定义启动资源\n加载场景预加载资源\n执行场景加载后回调
```
