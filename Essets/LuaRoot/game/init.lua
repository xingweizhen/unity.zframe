--
-- @file    game/init.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2019-05-22 23:24:20
-- @desc    在这里进行一些自定义的环境初始化
-- 

libunity.NewChild("/UIROOT", "Launch/Singleton")

local ENV = _G.ENV
print(serpent.block(ENV))

UE.Screen.sleepTimeout = "NeverSleep"
UE.Application.runInBackground = true
UE.Application.targetFrameRate = ENV.debug and -1 or 60
UE.QualitySettings.antiAliasing = ENV.debug and 8 or 0

rawset(_G, "TEXT", setmetatable({}, { __index = function(t, k) return k end }))

libui.key_event("Escape", function ()
	local Params = { title = UE.Application.productName, content = tostring(_G.TEXT.askQuitApp) }
    local MBox = libui.MBox
    if MBox.is_active(Params) then MBox.close(); return end
    if MBox.is_queued(Params) then return end

    MBox():set_params(Params)
        :set_param("block", true)
        :set_event(libunity.AppQuit)
        :show()
end)

libscene.add_level(0, function ()
	print("launching cb")
end, function () return { 
	{ path = "atlas/Common/", method = "Forever", },
	{ path = "ui/", method = "Forever", },
} end)

libscene.add_level("Demo", function ()
	print("Demo cb")
end)

-- ui.setenv("libgame", require "libgame.cs")
