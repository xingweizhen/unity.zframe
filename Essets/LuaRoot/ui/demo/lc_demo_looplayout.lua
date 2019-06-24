--
-- @file    ui/demo/lc_demo_looplayout.lua
-- @author  xingweizhen
-- @date    2019-06-07 08:42:44
-- @desc    DEMO_LoopLayout
--

local self = ui.new()
local _ENV = self
--!*[开始]自动生成函数*--

function on_vert_ent(go, i)
	ui.index(go, i + 1)
	ui.ref(go).lbText.text = "[#" .. i .. "]" .. Verts[i % #Verts + 1]
end

function on_hori_ent(go, i)
	ui.index(go, i + 1)
	ui.ref(go).lbText.text = i .. "\n" .. Verts[i % #Verts + 1]
end

function on_hori_grid_ent(go, i)
	ui.index(go, i + 1)
	ui.ref(go).lbText.text = i
end

function on_vert_grid_ent(go, i)
	ui.index(go, i + 1)
	ui.ref(go).lbText.text = i
end

function on_btnset_click(btn)
	local value = Ref.barSlider.value
	libugui.SetScrollValue(Ref.SubVert.go, value)
	libugui.SetScrollValue(Ref.SubHori.go, value)
	libugui.SetScrollValue(Ref.SubHGrid.go, value)
	libugui.SetScrollValue(Ref.SubVGrid.go, value)
end

function on_tglsize_click(tgl, value)
	local size = value and 500 or 300
	libugui.SetSizeDelta(Ref.SubVert.go, 300, size)
	libugui.SetSizeDelta(Ref.SubHori.go, size, 300)
	libugui.SetSizeDelta(Ref.SubHGrid.go, size, 300)
	libugui.SetSizeDelta(Ref.SubVGrid.go, 300, size)
end
--!*[结束]自动生成函数*--


function init_view()
	ui.group(Ref.SubVert.SubView.SubContent.GrpList)
	ui.group(Ref.SubHori.SubView.SubContent.GrpList)
	ui.group(Ref.SubHGrid.SubView.SubContent.GrpList)
	ui.group(Ref.SubVGrid.SubView.SubContent.GrpList)
	--!*[结束]自动生成代码*--
end

function init_logic()
	self.Verts = {
		"L", "Next\nNext", "San\nSan-Line\nNode",
	}
	
	libugui.SetScrollValue(Ref.SubVert.go, 0)
	libugui.SetLoopCap(Ref.SubVert.SubView.SubContent.GrpList.go, 100, true, true)
	libugui.SetLoopCap(Ref.SubHori.SubView.SubContent.GrpList.go, 100, true, true)
	libugui.SetLoopCap(Ref.SubHGrid.SubView.SubContent.GrpList.go, 100, true, true)
	libugui.SetLoopCap(Ref.SubVGrid.SubView.SubContent.GrpList.go, 100, true, true)
end

function show_view()
	
end

function on_recycle()
	
end

return self

