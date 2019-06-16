--
-- @file    framework/hotupdate.lua
-- @authors xing weizhen (kokohna@163.com)
-- @date    2019-06-15 20:44:04
-- @desc    描述 
-- 

local P = {}

function P.onchanged(path, changeType)
	if changeType.name == "Changed" then
		local pkgName = path:gsub(".lua$", "")
		local valueType = type(rawget(_G.PKG, pkgName))
		if valueType == "function" then
			_G.PKG[pkgName] = nil
			print("RESET `" .. pkgName .. "`")
		elseif valueType == "table" then

		end
	end
end

function P.onrenamed(path, oldPath)
	print(oldPath, "->", path)
end

return P