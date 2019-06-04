--
-- @file    framework/variable.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

do
    _G.libasset = require "libasset.cs"
    _G.libunity = require "libunity.cs"
    _G.libugui = require "libugui.cs"
    _G.libsystem = require "libsystem.cs"
    
    libunity.LogI = function ( ... ) libunity.Log(1, "I", ...) end
    libunity.LogD = function ( ... ) libunity.Log(1, "D", ...) end
    libunity.LogW = function ( ... ) libunity.Log(1, "W", ...) end
    libunity.LogE = function ( ... ) libunity.Log(1, "E", ...) end

    -- 公用元表
    _G.MT = {
        Const = {
            __newindex = function  (_, n, v)
                if n ~= "_" then
                    libunity.LogE("attempt to set undeclared variable {0} = {1}\n{2}", n, v, debug.traceback())
                end
            end,
            __index = function (_, n)
                libunity.LogE("attempt to get undeclared variable {0}\n{1}", n, debug.traceback())
            end,
        },
        ReadOnly = {
            __newindex = function  (_, n, v)
               error(string.format("尝试新增只读表字段 [%s] = %s", tostring(n), tostring(v)), 2)
            end,
        },
        AutoGen = {
            __index = function (t, n)
                local mt, v = rawget(t, "_mt")
                if mt then
                    v = mt.new and mt.new() or setmetatable({}, mt)
                else
                    v = {}
                end
                rawset(t, n, v)
                return v
            end,
        },
        KeyName = {
            __index = function (t, n) return n end,
        },
    }

    _G.UE = CS.UnityEngine

    _G.UGUI = CS.ZFrame.UGUI

    _G.System = CS.System

    -- 表
    _G.PKG = setmetatable({}, {
        __index = function (t, n)
            local v = dofile(n)
            if v then t[n] = v end
            return v
        end
    })
    _G.import = function (path, silent)
        local ret = _G.PKG[path:gsub(".lua$", "")]
        if ret == nil and (not silent) then
            print(string.format("%s not exist!\n%s", path, debug.traceback()))
        end
        return ret
    end

    -- 类
    _G.DEF = setmetatable({}, {
        __index = function (t, n)
            local name = string.lower(n)
            return _G.PKG["data/object/"..name]
        end
    })

    -- UI库(MessageBox, Toast, Tip, ...)
    _G.libui = {}

    -- JSON库
    _G.cjson = dofile("framework/tinyjson.lua")

    local Application = _G.UE.Application

    local platform = Application.platform.name
    -- 环境
    _G.ENV = {
        -- 平台名
        unity_platform = platform,
        app_data_path = Application.dataPath,

        app_persistentdata_path = "",
        app_streamingassets_path = "",
        using_assetbundle = false,

        debug = platform == "OSXEditor"
                or platform == "OSXPlayer"
                or platform == "WindowsEditor"
                or platform == "WindowsPlayer",
    }

    -- 常量数值表
    _G.CVar = {}
    setmetatable(_G.ENV, _G.MT.ReadOnly)

    local mtGO = {
        __name = "UnityObject",
        __tostring = function (self)
            return string.format("[GO:%s/%s<%s>]", self.root.name, self.path, self.com)
        end
    }
    _G.GO = function (root, path, com)
        if type(root) == "table" then
            return _G.GO(root.root, root.path .. "/" ..path, com)
        else
            return setmetatable({ root = root, path = path, com = com, }, mtGO)
        end
    end

    local errfunc = function (e)
        libunity.Log(nil, "E", "{0}\n{1}", e, debug.traceback())
        return e
    end
    _G.trycall = function (f, ...)
        return xpcall(f, errfunc, ...)
    end

    _G.newlog = function ()
        local ErrorLogs = {}
        return function (fmt, ...)
            if fmt then
                table.insert(ErrorLogs, string.format(fmt, ...))
            else
                if #ErrorLogs > 0 then
                    libunity.Log(1, "E", "\n{0}", table.concat(ErrorLogs, "\n"))
                end
            end
        end
    end

    -- 简单的记忆函数，永久记忆版
    _G.memoize = function (f)
        local mem = {}
        return function (x)
            local r = mem[x]
            if r == nil then
                r = f(x)
                mem[x] = r
            end
            return r
        end
    end

    -- 格式化的print函数
    _G.printf = function(fmt, ... )
        print(string.format(fmt, ...))
    end

    -- 显示名称＋编号
    _G.cfgname = function(Cfg)
        if _G.ENV.debug then
            return string.format("%s #%d", Cfg.name, Cfg.id)
        else return Cfg.name end
    end

    -- 其他扩展库
    dofile "framework/util/string"
    dofile "framework/util/table"
    dofile "framework/util/math"
    -- lua5.3已经支持位运算
    --dofile "framework/util/bit32.lua"
    dofile "framework/util/class.lua"
    dofile "framework/util/date.lua"
    dofile "framework/util/debug.lua"

    -- 内部类
    dofile "framework/util/client"
    dofile "framework/util/link"
    dofile "framework/util/queue"
    dofile "framework/util/stack"
    dofile "framework/util/tree"
    dofile "framework/util/pref"

    -- 动态数据管理库
    _G.libdata = dofile "framework/dydata"
    -- 定时器库
    _G.libtimer = dofile "framework/dytimer"

    -- 场景管理
    _G.libscene = import "framework/scenemgr"
    -- 网络管理
    _G.libnet = import "framework/networkmgr"
    _G.next_action = _G.PKG["framework/clock"].add_action
    _G.remove_action = _G.PKG["framework/clock"].remove_action    

    -- 音频
    dofile "framework/audmgr"

    -- UI扩展库
    import "framework/ui/window"
    dofile "framework/ui/keyevent"
    dofile "framework/ui/messagebox"
    dofile "framework/ui/toast"
    dofile "framework/ui/monotoast"
    dofile "framework/ui/netwaiting"
    -- dofile "ui/_tool/tweens.lua"


end
