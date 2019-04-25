--
-- @file 	framework/console/datatool.lua
-- @anthor  xing weizhen (xingweizhen@firedoggame.com)
-- @date	2016-05-12 14:31:21
-- @desc    描述
--

local OBJDEF = {}
OBJDEF.__index = OBJDEF
setmetatable(OBJDEF, _G.PKG["framework/console/help"])

function OBJDEF:list(...)
	local libsystem = require "libsystem.cs"
	local Args = {...}
	local Data = self.Data
	for _,v in ipairs(Args) do
		local n = tonumber(v) or v
		Data = Data[n]
		if not Data then break end
	end
	if type(Data) == "table" then
		local Hold = {}
		table.insert(Args, 1, self.name)
		table.insert(Hold, string.format("# %s = {", table.concat(Args, ".")))
		for k,v in pairs(Data) do
			if type(v) ~= "function" then
				table.insert(Hold, libsystem.StringFmt("{0,-20}{1,20}", tostring(k), tostring(v)))
			end
		end
		table.insert(Hold, "}")
		print(table.concat(Hold, "\n"))
	else
		print(table.concat(Args, ".").." = "..tostring(Data))
	end
end

function OBJDEF:clear()
	self.Data.clear()
end

return OBJDEF