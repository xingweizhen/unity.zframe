--
-- @file 	framework/util/bit32.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date	2016-02-29 11:11:58
-- @desc    描述
--

local libsystem = require "libsystem.cs"

local P = {}

function P.bor(a, b)
	return libsystem.BitOr(a, b)
end

function P.band(a, b)
	return libsystem.BitAnd(a, b)
end

_G.bit32 = P
