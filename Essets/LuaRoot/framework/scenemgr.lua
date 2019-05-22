--
-- @file 	framework/scenemgr.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date	2016-02-29 11:11:58
-- @desc    描述
--

local P = {}

-- 当场景加载完成时，参数levelName表示加载完成的场景名称
function P.on_level_loaded(levelName, launching)
	ui.clear_stack()
	g_current_level_name = levelName
	print("level loaded", levelName)

	local user_level_loaded = P.user_level_loaded
	if user_level_loaded then
		user_level_loaded(levelName, launching)
	end
end

return P
