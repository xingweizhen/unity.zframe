--
-- @file    framework/networkmgr.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2015-04-17 19:07:04
-- @desc    网络消息管理
--

import "framework/msgdef"

local tostring, pairs, table, string
    = tostring, pairs, table, string
local libunity = require "libunity.cs"

local UE_Time = UE.Time

local HTTP_Silent = {
    report = true,
    VERSION = true,
}
local CONNECT_TIMEOUT = 10
local ClientDEF = _G.DEF.Client
local MainCli = ClientDEF.get("GameTcp")
local GameCli = MainCli
-- HTTP回调
local HTTPHandler = {}
-- 下载回调
local DownloadCbf = {}

local P = {
    MainCli = MainCli,
}

function P.set_cli(cli)
    GameCli = cli or MainCli
end

function P.get_tcp() return GameCli.tcp end

function P.get(name)
    return ClientDEF.get(name)
end

-- 回调方法
function P.on_nc_init(mgr)
    GameCli:initialize()
    GameCli:set_handler(P.on_nc_receiving)
end

function P.on_nc_receiving(cli, nm)
    
end

function P.on_http_response(url, tag, resp, isDone, err)
    libui.Waiting.hide()
    local cbf = HTTPHandler[tag]
    if err then
        if not HTTP_Silent[tag] then libui.Toast.norm(_G.TEXT.tipConnectFailure) end
        libunity.LogW("Http Fail: [{0}:{1}]{2}; {3}", tag, url, resp, err)
    elseif not isDone then
        if not HTTP_Silent[tag] then libui.Toast.norm(_G.TEXT.tipConnectTimeout) end
        libunity.LogW("Http Timeout: [{0}:{1}]{2}", tag, url, resp)
    end
    if cbf then cbf(resp, isDone, err) end
end

function P.on_http_download(url, current, total, isDone, err)
    if err then libunity.LogW("Download from {0} Error:{1}", url, err) end

    local cbf = DownloadCbf[url]
    if cbf then cbf(url, current, total, isDone, err) end
end

-- ============================================================================

-- 启动一个HTTP POST
function P.http_post(tag, url, postData, headers, cbf)
    libnetwork.HttpPost(tag, url, postData, headers, CONNECT_TIMEOUT);
    if cbf then HTTPHandler[tag] = cbf end
end

-- 启动一个HTTP GET
function P.http_get(tag, url, param, cbf)
    libnetwork.HttpGet(tag, url, param, CONNECT_TIMEOUT);
    if cbf then HTTPHandler[tag] = cbf end
end

-- 开始一个HTTP下载
-- @url         远程文件地址
-- @savePath    下载保存位置
-- @cbf         下载过程回调
function P.http_download(url, savePath, md5chk, cbf)
    libnetwork.HttpDownload(url, savePath, md5chk, CONNECT_TIMEOUT)
    if cbf then DownloadCbf[url] = cbf end
end

-- ============================================================================
-- 建立连接
function P.connect(host, port, onConnected, onInterrupt, onBroken)
    MainCli:set_connected(onConnected)
    MainCli:set_event(onInterrupt, onBroken)
    if MainCli:connected() then
        onConnected()
    else
        MainCli:connect(host, port)
    end
end

-- 客户端断开连接
function P.disconnect(name, keepHost)
    if name then
        local cli = ClientDEF.find(name)
        if cli then cli:disconnect(keepHost) end
    else
        GameCli:disconnect(keepHost)
        if GameCli ~= MainCli then MainCli:disconnect(keepHost) end
    end
end

-- 执行某个操作前检查是否wifi连接
function P.check_internet(action)
    local network = UE.Application.internetReachability
    if network == "ReachableViaLocalAreaNetwork" then
        if action then action() end
    else
        -- 非wifi网络
        libui.MBox()
            :set_param("content", _G.TEXT.tipAskUpdateViaCarrierDataNetwork)
            :set_event(action)
            :show()
    end
end

-- ============================================================================
-- 创建一个消息对象
P.msg = _G.DEF.Client.msg
P.gamemsg = _G.DEF.Client.gamemsg

-- 客户端发送消息
function P.send(nm, msg)
    GameCli:send(nm, msg)
end

-- 用于判断是否使用本地数据中
function P.enabled()
    local Player = libdata.get_player()
    return Player ~= nil and Player.id ~= 0
end

-- ============================================================================
-- 注册消息分析器
-- 一个消息只能注册一次
function P.regist(code, handler)
    GameCli.regist_global(code, handler)
end

-- 订阅消息
function P.subscribe(code, handler)
    GameCli.subscribe_global(code, handler)
end

-- 取消订阅
function P.unsubscribe(code, handler)
    GameCli.unsubscribe_global(code, handler)
end

-- 发布消息
function P.broadcast(code, Ret)
    GameCli:broadcast(code, Ret)
end

return P
