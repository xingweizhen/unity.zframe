--
-- @file    framework/ui/keyevent.lua
-- @authors xing weizhen (kokohna@163.com)
-- @date    2019-05-25 16:08:08
-- @desc    描述 
-- 

local P = _G.libui

local KeyEvents = {}

function P.key_event(key, event)
    if event then
        KeyEvents[key] = event
    else 
        return KeyEvents[key] 
    end
end
