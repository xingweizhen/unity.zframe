--
-- @file    framework/tinyjson.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

-- local libtool = require "libtool.cs"

-- local OBJDEF = { _VERSION = "20150413.23" }

-- function OBJDEF.encode(T, prettyPrinted)
-- 	return libtool.TableToJSON(T, prettyPrinted)
-- end

-- function OBJDEF.decode(json)
-- 	return libtool.JSONToTable(json)
-- end

-- return OBJDEF

local P = {}

local JSON = assert(loadfile "framework/JSON.lua")()

function JSON:assert(message)
	libunity.LogE(message .. "\n" .. debug.traceback())
end

function JSON:onDecodeError(message, text, location, etc)
	libunity.LogW("[LuaDecodeError]".. message .."\ntext:"..text)
end

function JSON:unsupportedTypeEncoder(value)
	return tostring(value)
end

function P.encode(T, prettyPrinted)
	if prettyPrinted then
		return JSON:encode_pretty(T)
	else
		return JSON:encode(T)
	end
end

function P.decode(json)
	return JSON:decode(json)
end

return P
