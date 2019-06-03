--
-- @file    framework/console/nettool.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

local P = {}

function P:connect(host, strPort)
    local port = tonumber(strPort)
    libnet.connect(host, port)
end

function P:close()
    libnet.disconnect()
end

return P
