--
-- @file    framework/datamgr/dydata.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

local P = setmetatable({}, _G.MT.AutoGen)

function P.find_obj(tName, key, value)
	local Objs = rawget(P, tName)
	if Objs then
		local finder = Objs.finder or _G.pairs
		for k,v in finder(Objs) do
			if v[key] == value then return v end
		end
	end
end

function P.find_objs(tName, key, value)
	local Objs = rawget(P, tName)
	if Objs then
		local Rets = {}
		local finder = Objs.finder or _G.pairs
		for _,v in finder(Objs) do
			if v[key] == value then 
				table.insert(Rets, v)
			end
		end
		return Rets
	end
end

function P.clear()
    for k,v in pairs(P) do
        if type(v) ~= "function" then P[k] = nil end
    end

    if P.reset then P:reset() end
end

_G.DY_DATA = P
