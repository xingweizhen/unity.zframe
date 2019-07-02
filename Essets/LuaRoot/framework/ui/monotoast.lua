--
-- @file 	framework/ui/monotoast.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date	2016-03-16 10:19:34
-- @desc    描述
--

local ToastGroups = {}

local function get_toast_group(group)
	local Group = ToastGroups[group]
	if Group == nil then
		Group = {
			Queue = _G.DEF.Queue.new(),
		}
		ToastGroups[group] = Group
	end
	return Group
end

local function on_fade_out(go)
	libunity.Recycle(go)
	for _,v in pairs(ToastGroups) do
		if v.Curr and v.Curr.Ref.go == go then
			v.Curr = nil
		end
	end
end

local function invoking_toast(group)
	local ToastGroup = ToastGroups[group]
	local ToastQueue = ToastGroup.Queue

    while ToastQueue:count() > 0 do
    	local Toast = ToastQueue:peek()
    	local CurrToast = ToastGroup.Curr
    	if CurrToast == nil or CurrToast ~= Toast then
    		if CurrToast then
	    		libugui.DOTween(CurrToast.Ref.go, -1, on_fade_out)
	    	end
	    	ToastGroup.Curr = Toast

    		CurrToast = Toast
	    	CurrToast:start()
	    	coroutine.yield(CurrToast.stay)
	    end
    	ToastQueue:dequeue()
	end
end

local InitFunctions = {
	Icon = function (self)
		local Ref = self.Ref
		Ref.lbTips.text = self.args.tips
		Ref.lbTips.color = self.color or "#C5C5C5"
		Ref.spIcon:SetSprite(self.args.icon)
		Ref.spIcon.color = self.color or "#FFFFFF"
		libugui.DOTween(Ref.go, 1, on_fade_out, true)
	end,
}

local DefCanvas = {
	Play = 1,
	Icon = 1,
}

local DefDepth = {

}
--=============================================================================
local OBJDEF = {}
OBJDEF.__index = OBJDEF
OBJDEF.__eq = function (a, b)
	return a.style == b.style and a.args == b.args
end

function OBJDEF:start()
	local go = ui.create("UI/"..self.style.."Toast", self.depth, self.canvas)
    self.Ref = ui.ref(go)
	self:init()
end

function OBJDEF:init()
	local Ref = self.Ref
	Ref.lbTips.color = self.color or "#C5C5C5"
	Ref.lbTips.text = self.args
	if Ref.spIcon then
		if self.icon then
			Ref.spIcon:SetSprite(self.icon)
		end
	end
	libugui.SetAlpha(Ref.go, 0)
	libugui.DOTween(Ref.go, 1, on_fade_out, true)
end

function OBJDEF:show(stay, color, group)
	self.stay = stay
	self.color = color
	if group == nil then group = 0 end

	local ToastQueue = get_toast_group(group).Queue
	ToastQueue:enqueue(self)
	if ToastQueue:count() == 1 then
		libunity.StartCoroutine(nil, invoking_toast, group)
	end
end

function OBJDEF.clear(style)
	if style == nil then
		for _,v in pairs(ToastGroups) do
			v.Queue:clear()
		end
	else
		for _,v in pairs(ToastGroups) do
			for _,Toast in ipairs(v.Queue) do
				if Toast.style == style then Toast.ignore = true end
			end
		end
	end
end

setmetatable(OBJDEF, { __call = function (_, style, args, depth, canvas, icon)
    if style == nil then style = "Norm" end
    return setmetatable({
    	args = args,
    	style = style,
    	init = InitFunctions[style],
    	depth = depth or DefDepth[style],
    	canvas = canvas or DefCanvas[style],
    	icon = icon,
    }, OBJDEF)
end})

_G.libui.MonoToast = OBJDEF
