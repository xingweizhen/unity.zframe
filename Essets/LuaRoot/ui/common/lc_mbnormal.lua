--
-- @file    ui/common/lc_mbnormal.lua
-- @author  xingweizhen
-- @date    2019-05-26 14:15:21
-- @desc    MBNormal
--

local self = ui.new()
local _ENV = self
--!*[开始]自动生成函数*--

function on_submain_subop_btnconfirm_click(btn)
	libui.MBox.on_btnaction("confirm")
end

function on_submain_subop_btncancel_click(btn)
	libui.MBox.on_btnaction("cancel")
end
--!*[结束]自动生成函数*--

function init_view()
	--!*[结束]自动生成代码*--
end

function init_logic()
	local SubMain = Ref.SubMain
	SubMain.lbTitle.text = Context.title
	SubMain.lbDesc.text = Context.content
end

function show_view()
	
end

function on_recycle()
	
end

return self

