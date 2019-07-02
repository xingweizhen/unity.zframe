-- File Name : framework/ui/toast.lua

-- 浮动提示框
-- libui.Toast.make(nil, "提示框框框"):show()

local ToastQueue = {}

local function on_fade_end(go)
    libunity.Recycle(go)
    for i,v in ipairs(ToastQueue) do
        if v.Ref.go == go then
            if v.on_hide then v.on_hide() end
            table.remove(ToastQueue, i)
        break end
    end
end

local function coro_sorting(self)
    local Vector3 = UE.Vector3
    local goSelf = self.Ref.go
    local height = libugui.GetRectSize(goSelf).y + 20
    for i=#ToastQueue,1,-1 do
        local v = ToastQueue[i]
        local root = v.Ref.go
        if goSelf ~= root and root.activeInHierarchy then
            local pos = libugui.GetAnchoredPos(root)
            local siz = libugui.GetRectSize(root)
            local tar = Vector3(pos.x, height, pos.z)
            height = height + siz.y + 20
            libugui.KillTween(root)
            libugui.DOTween("TweenPosition", root, nil, tar, { duration = 0.2, })
        else
            table.remove(ToastQueue, i)
        end
    end

    table.insert(ToastQueue, self)
end

--=============================================================================

local OBJDEF = { }
OBJDEF.__index = OBJDEF

function OBJDEF.make(style, args)
    if style == nil then style = "Norm" end
    return setmetatable({ args = args, style = style }, OBJDEF)
end

function OBJDEF:init(canvas)
    local Last = ToastQueue[#ToastQueue]
    -- 忽略掉和前一个相同的
    if Last and Last.args == self.args then return end

    local go = ui.create("UI/"..self.style.."Toast", nil, canvas)
    if go then
        local Ref = ui.ref(go)
        local lbTips = Ref.lbTips
        if lbTips then lbTips.text = self.args end
        libugui.DOTween(Ref.go, 1, on_fade_end, true)
        return Ref
    end
end

function OBJDEF:show(canvas)
    local Ref = self:init(canvas)
    if Ref then
        self.Ref = Ref
        libunity.Invoke(Ref.go, 0, function () coro_sorting(self) end)
    else
        print("Toast", self.style, self.args)
    end
end

function OBJDEF:set_event(cbf)
    self.on_hide = cbf
    return self
end

function OBJDEF.clear()
    table.clear(ToastQueue)
end

function OBJDEF.norm(text)
    OBJDEF.make("Norm", text):show()
end

-- function OBJDEF.label(text, color, pos)
--     local go = libunity.AddChild("/UIROOT/UICanvas", "UI/TIPLabel")
--     local lb = go:GetComponent("UILabel")
--     if pos then
--         libugui.SetAnchoredPos(lb, pos)
--     end
--     lb.text = text
--     lb.color = color
--     libugui.DOTween(go, 1)
-- end

setmetatable(OBJDEF, { __call = function (_, style, args)
    if style == nil then style = "Norm" end
    return setmetatable({ args = args, style = style }, OBJDEF)
end})

_G.libui.Toast = OBJDEF
