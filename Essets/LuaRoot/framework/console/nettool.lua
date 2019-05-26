--
-- @file    framework/console/nettool.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

local P = {}

function P:chhost(tag)
    _G.PKG["network/channel"].update_host(tag)
end

function P:connect(host, strPort)
    local port = tonumber(strPort)
    local NW = import "network/networkmgr"
    NW.connect(host, port)
end

function P:enter(index)
    local LOGIN = _G.PKG["network/login"]
    LOGIN.enter_server(tonumber(index))
end

function P:logout()
    local LOGIN = _G.PKG["network/login"]
    LOGIN.logout()
end

function P:close()
    local NW = _G.PKG["network/networkmgr"]
    NW.disconnect()
end

function P:pid(pid)
    _G.PKG["libmgr/login"].Channel.pid = tonumber(pid)
    _G.UI.Toast.make(nil, "已修改渠道号为："..pid):show()
end

function P:reset_cli()
    local DY_DATA = _G.PKG["datamgr/dydata"]
    DY_DATA.CliData = nil

    local NW = _G.PKG["network/networkmgr"]
    NW.send(NW.msg("COMMON.CS.CLIENT_DATA_SET"):writeString(""))
end

function P:invite(pid)
    _G.NW.TEAM.invite(tonumber(pid))
end

function P:attackhome(pid)
    _G.NW.MULTI.attack_player_home(0, pid, 2)
end

function P:loc(from, to, text)
    local EscapeURL = UE.WWW.EscapeURL
    local appId = "1000"
    local secrectKey = "d9e23d93053f49ade2f8fce185acedd4"
    local uri = "https://translate.funplusgame.com/api/v2/translate"
    local params = { appId = appId, q = text, source = from, target = to, timeStamp = os.secs2date("%Y-%m-%dT%H:%M:%SZ") }
    local EncodeParams = {}
    for k,v in pairs(params) do
       table.insert(EncodeParams, string.format("%s=%s", EscapeURL(k), EscapeURL(v)))
    end
    table.sort(EncodeParams)

    local textParams = table.concat(EncodeParams, "&")
    local string2sign = string.format("POST\ntranslate.funplusgame.com\n/api/v2/translate\n%s", textParams)
    local Header = {
        Accept = "application/json;charset=UTF-8",
        Authorization = CS.CMD5.Token(string2sign, secrectKey)
    }
    NW.http_post("TRANSLATE", uri, textParams, Header, function (resp, isDone, err)
        print(resp, isDone, err)
    end)
end

function P:download_all_remote_assets()
    local LFL = _G.ENV.LFL
    if LFL then
        local Assets = LFL.Assets
        local RemoteAssets = {}
        for k,v in pairs(Assets) do
            if v.crc then
                table.insert(RemoteAssets, k .. "/")
            end
        end
        libasset.PreDownload(RemoteAssets)
    end
end

return P
