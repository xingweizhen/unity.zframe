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

function UI.KeyNotify.Escape ()
	print("Escape")
    -- local TEXT = _G.TEXT
    -- local Alert = {
    --     title = UE.Application.productName,
    --     message = tostring(TEXT.askQuitApp),
    --     icon = "app_icon",
    -- }
    -- DY_DATA.AlertCBF["1"] = function ()
    --     libunity.AppQuit()
    -- end
    --libunity.SendMessage("/UIROOT/Singleton", "AlertMessage", cjson.encode(Alert))
	local Params = { title = UE.Application.productName, content = tostring(_G.TEXT.askQuitApp) }
    local MB = _G.UI.MBox
    if MB.is_active(Params) then MB.close(); return end
    if MB.is_queued(Params) then return end
    MB.make():set_params(Params)
        :set_param("block", true)
        :set_event(libunity.AppQuit)
        :show()
end

-- ui.setenv("libgame", require "libgame.cs")
