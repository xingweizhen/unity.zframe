--
-- @file    framework/console/uitool.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

local libunity = require "libunity.cs"
local libugui  = require "libugui.cs"
local libasset = require "libasset.cs"

local P = {}

function P:capturemode(arg)
    arg = arg:lower()
    if arg == "on" then
        arg = false
    elseif arg == "off" then
        arg = true
    end

    local uiroot = libunity.Find("/UIROOT")
    libunity.SetActive(GO(uiroot, "UICamera3rd"), arg)
    libunity.SetActive(GO(uiroot, "UICamera2nd"), arg)
    libunity.SetActive(GO(uiroot, "RoleCamera"), arg)
    libunity.SetActive(GO(uiroot, "UICamera"), arg)
end

function P:open(prefab, depth)
    ui.open("UI/" .. prefab, tonumber(depth), libui.DebugContexts and libui.DebugContexts[prefab])
end

function P:show(prefab, depth)
    ui.show("UI/" .. prefab, tonumber(depth), libui.DebugContexts and libui.DebugContexts[prefab])
end

function P:close(name)
    ui.close(name)
end

function P:loc(lang)
    ui.setloc(nil, lang)

    for _,Wnd in ui.foreach_lcwnds() do
        if Wnd.Ref then
            Wnd.go:BroadcastMessage("InitText")
            trycall(Wnd.init_logic)
        end
    end
end

function P:toast(style, text)
    libui.Toast(style, text):show()
end

function P:monotoast(style, text)
    libui.MonoToast(style, text):show(0.5)
end

function P:mbox(content)
    libui.MBox()
        :set_param("content", content)
        :show()
end

function P:mbshow(prefab)
    local Params = libui.DebugContexts and libui.DebugContexts[prefab]
    libui.MBox(prefab):set_params(Params):show()
end

return P
