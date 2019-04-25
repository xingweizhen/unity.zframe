--
-- @file    framework/console/battletool.lua
-- @anthor  xing weizhen (xingweizhen@rongygame.com)
-- @date    2015-12-24 12:06:48
-- @desc    描述
--

local P = {}


function P:effect(defenser, effId)
    local libbattle = require "libbattle.cs"
    local LVEffects = _G.CFG.SkillLib.Effects[tonumber(effId)]
    local Eff = LVEffects and LVEffects[1] or nil
    if Eff == nil then
        local libunity = require "libunity.cs"
        libunity.LogE("不存在的功能={0}", effId)
    return end
    local attacker = libbattle.GetFormation().Units[1]
    libbattle.CastEffect(attacker, defenser, { Eff })
end

function P:event(evtType, ...)
    local LC = _G.PKG["ui/battle/lc_frmbattle"]
    if evtType ~= "_" then
        LC.sync_battle("ntf_custom_event", evtType, ...)
    else
        LC.sync_battle("ntf_custom_event", "", ...)
    end
end

function P:create(id, camp)

end

function P:gender()
    local Self = DY_DATA.Self
    Self.gender = 3 - Self.gender
    Self.datDirty = true
    _G.PKG["game/ctrl"].create(Self)
end

function P:orbit()
    local cam = UE.Camera.main
    if cam then
        local enabled = libunity.IsEnable(cam, "MouseOrbit")
        libunity.SetEnable(GO(cam, "", "MouseOrbit"), not enabled)
    end
end

return P
