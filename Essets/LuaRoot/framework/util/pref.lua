--
-- @file    framework/util/pref.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2018-05-20 12:58:41
-- @desc    描述
--

local PlayerPrefs = UE.PlayerPrefs
local OBJDEF = {}
OBJDEF.__index = OBJDEF

function OBJDEF.new(name)
	return setmetatable({ name = name, }, OBJDEF)
end

function OBJDEF:onload(cbf)
	self.onload = cbf
end

function OBJDEF:onsave(cbf)
	self.onsave = cbf
end

function OBJDEF:save(key, value)
	if self.Data then
		if key then
			self.Data[key] = value
		end

		local str = cjson.encode(self.Data)
		PlayerPrefs.SetString(self.name, str)
		PlayerPrefs.Save()
		if self.onsave then self.onsave(self.Data) end
	end
end

function OBJDEF:load()
	if self.Data == nil then
		local str = PlayerPrefs.GetString(self.name)
		self.Data = #str > 0 and cjson.decode(str) or {}
	end
	if self.onload then self.onload(self.Data) end
	return self.Data
end

function OBJDEF:clear()
	PlayerPrefs.DeleteKey(self.name)
end

_G.DEF.Pref = OBJDEF
