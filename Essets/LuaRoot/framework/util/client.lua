--
-- @file    framework/util/client.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-01-04 12:07:13
-- @desc    客户端类
--

local libunity = require "libunity.cs"
local libnetwork = require "libnetwork.cs"
local MSG = _G.PKG["framework/msgdef"]

local MAX_RECONNECT = 3
local CONNECT_TIMEOUT = 10

local NmDEF = {}
NmDEF.__index = NmDEF
NmDEF.__tostring = function (self)
    local nmType, msgName, size = self.type, nil, self.size
    if self.rsp then
        msgName = MSG.get_rsp_name(nmType) or ""
    else
        msgName = MSG.get_req_name(nmType) or ""
    end
    return string.format("[%s#%d %d bytes]", msgName, nmType, size)
end

function NmDEF:readU32() return libnetwork.ReadU32(self) end
function NmDEF:readU64() return libnetwork.ReadU64(self) end
function NmDEF:readFloat() return libnetwork.ReadFloat(self) end
function NmDEF:readString() return libnetwork.ReadString(self) end
function NmDEF:readBuffer() return libnetwork.ReadBuffer(self) end
function NmDEF:readArray(Array, unpacker, ...)
    local n = self:readU32()
    for i=1,n do
        local Elm = unpacker(self, ...)
        if Elm then table.insert(Array, Elm) end
    end
    return Array, n
end

function NmDEF:writeU32(i) libnetwork.WriteU32(self, i); return self end
function NmDEF:writeU64(l) libnetwork.WriteU64(self, l); return self end
function NmDEF:writeString(s) libnetwork.WriteString(self, s); return self end
function NmDEF:writeBuffer(b) libnetwork.WriteBuffer(self, b); return self end
function NmDEF:writeArray(Array, packer, ...)
    local n = #Array
    self:writeU32(n)
    for i=1,n do
        packer(self, Array[i], ...)
    end
    return self
end

local gnm = setmetatable({ size = 0 }, NmDEF)

-- 不等待返回的消息列表
local NoResponse = {}
-- 静默消息列表：不打印日志
local SilentNmSC = {}
-- 忽略返回状态影响：默认情况下返回消息会取消网络等待标志
local IgnoreNmSc = {}

local Clients = {}

local OBJDEF = { }
OBJDEF.__index = OBJDEF
OBJDEF.__tostring = function(self)
    return string.format("[%s]", self.name)
end

local on_tcp_connected, on_tcp_discnnected, on_tcp_recieving

-- local function log(fmt, ... )
--     libunity.LogD("[NW] " .. fmt, ...)
-- end
local log = libunity.LogD

local function chk_msg_type(code, rsp)
    local id = code
    if type(code) == "string" then
        if rsp then 
            id = MSG.get_rsp_type(code)
        else
            id = MSG.get_req_type(code)
        end
        if id == nil then
            libunity.LogE("错误的消息ID={0}", code)
            return
        end
    end
    return id
end
OBJDEF.chk_msg_type = chk_msg_type

-- @ TcpClient的回调
local function callback(self, name, ...)
    local cbf = self[name]
    if cbf then cbf(self, ...) end
end

on_tcp_connected = function (cli)
    libui.Waiting.hide()
    local self = Clients[cli]
    self.nReconnect = nil
    callback(self, "on_connected")
end

on_tcp_discnnected = function (cli)
    if cli.Error == nil then
        libunity.LogW("{0} disconnect: Timeout", cli.name)
    else
        libunity.LogW("{0} disconnect: {1}", cli.name, cli.Error)
    end
    local self = Clients[cli]
    if self.on_disconnected then
        callback(self, "on_disconnected")
    else
        if self.host then
            libunity.Invoke(nil, 1, function () self:reconnect() end)
        end
    end
end

on_tcp_recieving = function (cli, type, bodySize, size)
    gnm.rsp = true
    gnm.type, gnm.bodySize, gnm.size = type, bodySize, size

    local id = type
    local self = Clients[cli]
    local handle = self.Unpackers[id] or MSG.unpacker
    local msgName = tostring(gnm)
    if handle then
        -- 静默，不输出日志
        local silent = SilentNmSC[id]
        if not silent then log("{1} <-- {0}", msgName, self) end
        local Ret = handle(gnm, self)
        local n = self:broadcast(id, Ret, self)
        if not silent and n then log("{2}   > {0} x{1}", msgName, n, self) end
    else
        log("<color=red>miss handler for: {0} </color>", msgName)
    end

    callback(self, "on_recieving", gnm)

    if not IgnoreNmSc[id] then
        libui.Waiting.hide(id)
    end
end

local function do_connect(self, timeout, duration)
    local host, port = self.host, self.port
    self.tcp:Connect(host, port, timeout)
    if not self.background then
        libui.Waiting.show(_G.TEXT.tipConnecting, 0, duration)
    end
end

local function regist_unpacker(Unpackers, code, handler)
    local id = chk_msg_type(code, true)
    if id == nil then return end

    local cbf = Unpackers[id]
    if cbf == nil then
        Unpackers[id] = handler
    else
        libunity.LogW("消息[{0}({1})]已经被注册！请订阅该消息", code, id)
        print(debug.traceback())
    end
end

local function insert_handler(Dispatcher, code, handler)
    for _,v in ipairs(Dispatcher) do
        if v == handler then
            libunity.LogW("{0}已订阅{1}", handler, code)
            print(debug.traceback())
        return end
    end

    table.insert(Dispatcher, handler)
end

local function remove_handler(Dispatcher, handler)
    for i,v in ipairs(Dispatcher) do
        if v == handler then table.remove(Dispatcher, i) break end
    end
end

local function regist_dispatcher(Dispatchers, code, handler)
    local id = chk_msg_type(code, true)
    if id == nil then return end

    local hType = type(handler)
    local Dispatcher = Dispatchers[id]
    if Dispatcher == nil then
        if hType == "function" then
            Dispatchers[id] = { handler }
        elseif hType == "table" then
            Dispatchers[id] = handler
        end
    else
        if hType == "function" then
            insert_handler(Dispatcher, code, handler)
        elseif hType == "table" then
            for _,v in ipairs(handler) do
                insert_handler(Dispatcher, code, v)
            end
        end
    end
end

local function unregist_dispatcher(Dispatchers, code, handler)
    local id = chk_msg_type(code, true)
    if id == nil then return end

    local Dispatcher = Dispatchers[id]
    if Dispatcher then
        local hType = type(handler)
        if hType == "function" then
            remove_handler(Dispatcher, handler)
        elseif hType == "table" then
            for _,v in ipairs(handler) do
                remove_handler(Dispatcher, v)
            end
        end
    end
end

local GlobalUnpackers = {}
GlobalUnpackers.__index = GlobalUnpackers

local GlobalDispatchers = {}
GlobalDispatchers.__index = GlobalDispatchers

-- @ 类实现
local OBJS = setmetatable({}, {
    __index = function (t, n)
            local self = {
                name = n,
                -- 消息解析处理函数表
                Unpackers = setmetatable({}, GlobalUnpackers),
                -- 消息分发函数表
                Dispatchers = setmetatable({}, GlobalDispatchers),
            }

            t[n] = setmetatable(self, OBJDEF)
            return self
        end,
    })

function OBJDEF.msg(code, size)
    local id = chk_msg_type(code, false)
    if id == nil then return end

    libnetwork.NewNetMsg(id, size or 1024)
    gnm.type = id
    gnm.rsp = false
    return gnm
    -- local NetMsg = CS.clientlib.net.NetMsg
    -- return NetMsg.createMsg(id, size or 1024)
end

function OBJDEF.gamemsg(code, size)
   local id = chk_msg_type(code, false)
    if id == nil then return end

    libgame.NewNetMsg(id, size or 1024)
    gnm.type = id
    gnm.rsp = false
    return gnm
end

function OBJDEF.get(name)
    return OBJS[name]
end

function OBJDEF.find(name)
    return rawget(OBJS, name)
end

function OBJDEF.regist_global(code, handler)
    regist_unpacker(GlobalUnpackers, code, handler)
end

function OBJDEF.subscribe_global(code, handler)
    regist_dispatcher(GlobalDispatchers, code, handler)
end

function OBJDEF.unsubscribe_global(code, handler)
    unregist_dispatcher(GlobalDispatchers, code, handler)
end

-- 定义一个单向消息(仅Client->Server，不需要等待其返回)
function OBJDEF.noresponse(code)
    local id = chk_msg_type(code, false)
    if id then NoResponse[id] = true end
end

-- 定义一个单向消息(仅Server->Client，不会取消网络等待状态)
function OBJDEF.norequest(code)
    local id = chk_msg_type(code, true)
    if id then IgnoreNmSc[id] = true end
end

-- 定义一个静默消息（没有日志）
function OBJDEF.nolog(code, rsp)
    local id = chk_msg_type(code, rsp)
    if id then SilentNmSC[id] = true end
end

function OBJDEF:initialize()
    if self.tcp == nil then
        local tcp = libnetwork.GetTcpHandler(self.name)
        tcp.onConnected = on_tcp_connected
        tcp.onDisconnected = on_tcp_discnnected
        tcp.messageHandler = on_tcp_recieving
        self.tcp = tcp

        Clients[tcp] = self
        log("<color=yellow>网络模块初始化：</color> {0}", self)
    end
    return self
end

function OBJDEF:connect(host, port)
    self.host, self.port = host, port
    self.nReconnect = MAX_RECONNECT
    self:reconnect()
end

function OBJDEF:reconnect()
    if self:connected() then return end
    if self.host == nil then return end

    if self.nReconnect == nil then
        callback(self, "on_interrupt")
    else
        -- 计数并进行连接
        local nReconnect = self.nReconnect - 1
        self.nReconnect = nReconnect
        if nReconnect >= 0 then
             local timeout = CONNECT_TIMEOUT - nReconnect * 3
             local duration = (timeout + CONNECT_TIMEOUT) * (nReconnect + 1) / 2
             do_connect(self, timeout, duration)
        else
            self.host, self.port = nil, nil
            callback(self, "on_broken")
        end
    end
end

function OBJDEF:send(nm, msg)
    if nm and self.tcp.IsConnected then
        nm.size = libnetwork.SendNetMsg(self.tcp, nm)

        local post = NoResponse[nm.type]
        local silent = SilentNmSC[nm.type]
        if post then
            if not silent then
                log("{1} --> {0}", nm, self)
            end
        else
            if not self.background then
                libui.Waiting.show(nil, nil, nil, chk_msg_type(msg, true))
            end
            if not silent then
                log("{1} ==> {0}", nm, self)
            end
        end
    else
        libunity.LogW("发生消息[{0}]失败！连接?={1}", nm, self.tcp.IsConnected)
    end
end

function OBJDEF:disconnect(keepHost)
    if not keepHost then
        self.host, self.port = nil, nil
    end
    self.tcp:Disconnect()
end

function OBJDEF:connected()
    return self.tcp.IsConnected
end

function OBJDEF:get_error()
    return self.tcp.Error
end

-- @ 该连接接收消息时的回调
function OBJDEF:set_handler(handler)
    self.on_recieving = handler
    return self
end

-- @ 该连接建立成功时的回调
function OBJDEF:set_connected(onTcpConnected)
    self.on_connected = onTcpConnected
    return self
end

function OBJDEF:set_disconnected(onTcpDisconnected)
    self.on_disconnected = onTcpDisconnected
    return self
end

-- @ onInterrupt 该连接从连接状态异常中断时的回调
-- @ onBroken 该连接无法建立时的回调
function OBJDEF:set_event(onInterrupt, onBroken)
    self.on_interrupt = onInterrupt
    self.on_broken = onBroken
end

-- 注册消息分析器
-- 一个消息只能注册一次
function OBJDEF:regist(code, handler)
    regist_unpacker(self.Unpackers, code, handler)
end

-- 订阅消息
function OBJDEF:subscribe(code, handler)
    regist_dispatcher(self.Dispatchers, code, handler)
end

-- 取消订阅
function OBJDEF:unsubscribe(code, handler)
    unregist_dispatcher(self.Dispatchers, code, handler)
end

-- 发布消息
function OBJDEF:broadcast(code, ...)
    local id = chk_msg_type(code, true)
    local Dispatcher = self.Dispatchers[id]
    if Dispatcher then
        -- 自动发布订阅消息
        local n = 0
        for i=#Dispatcher,1,-1 do
            local status, ret = true, true
            local dispatch = Dispatcher[i]
            if dispatch then
                n = n + 1
                status, ret = trycall(dispatch, ...)
                if status and ret then Dispatcher[i] = false end
            end
        end

        return n
    end
end

_G.DEF.Client = OBJDEF
