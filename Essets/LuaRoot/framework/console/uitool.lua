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
    ui.open("UI/" .. prefab, tonumber(depth), UI.DebugContexts and UI.DebugContexts[prefab])
end

function P:show(prefab, depth)
    ui.show("UI/" .. prefab, tonumber(depth), UI.DebugContexts and UI.DebugContexts[prefab])
end

function P:close(name)
    ui.close(name)
end

function P:loc(lang)
    libugui.SetLocalize(lang, "en")
    ui.setloc(nil, lang)
    config("textlib").reset()

    for _,Wnd in ui.foreach_lcwnds() do
        if Wnd.Ref then
            Wnd.go:BroadcastMessage("InitText")
            trycall(Wnd.init_logic)
        end
    end
end

function P:scene(name)
    local SCENE = _G.SCENE
    if name == "Main" then
        SCENE.load_main()
    elseif name == "Login" then
        SCENE.load_login()
    elseif name == "intro" then
        SCENE.load_intro_level()
    else
        SCENE.load_stage({ path = name })
    end
end

function P:toast(style, text)
    UI.Toast.make(style, text):show()
end

function P:monotoast(style, text)
    UI.MonoToast.make(style, text):show(0.5)
end

function P:mbox(content)
    UI.MBox.make()
        :set_param("content", content)
        :show()
end

function P:mbshow(prefab)
    local Params = UI.DebugContexts[prefab]
    UI.MBox.make(prefab):set_params(Params):show()
end

function P:lmtbox(content, mode)
    local Box = UI.MBox.make("MBTimeLimit")
        :set_param("time", 3)
        :set_param("content", content)
        :set_param("mode", mode or "cancel")

    if mode == "close" then
        Box:set_param("Options", {
            { text = "option1" }, { text = "option2" }, { text = "option3" }
        })
    end
    Box:show()
end

P.guide = {}

function P.guide:launch(group)
    _G.PKG["guide/api"].launch(group, 1, true)
end

return P
