--
-- @file    ui/misc/lc_frmconsole.lua
-- @author  xingweizhen
-- @date    2019-06-07 07:59:28
-- @desc    FRMConsole
--

local self = ui.new()
local _ENV = self
--!*[开始]自动生成函数*--

function on_inpcmd_submit(inp, text)
	_G.PKG["framework/console/console"].parse_cmd(text)
	self:close()
end
--!*[结束]自动生成函数*--

function init_view()
	--!*[结束]自动生成代码*--
end

function init_logic()
	libugui.Select(Ref.inpCmd, true)
end

function show_view()
	
end

function on_recycle()
	
end

return self

