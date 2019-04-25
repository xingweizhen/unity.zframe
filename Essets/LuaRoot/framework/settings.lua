-- File Name : datamgr/settings.lua

local P = {
	LOW_SCREEN_H = UE.Application.isMobilePlatform and 720 or 600,
}

local PerformanceSets = {
	-- 高清配置
	HD = {
		heroMat = "NormMat_HD",
		light = true,
		shadow = "Projector",
		resH = 0,
		fxLevel = 999,
	},
	SD = {
		heroMat = "NormMat_SD",
		light = false,
		shadow = "Projector",
		resH = P.LOW_SCREEN_H,
		fxLevel = 999,
	},

}

local Performance
local SystemSettings

function P.load()
	local DefSettings = {
		videoQuality = "HD",
		Audio = {},
		Pushing = {},
	}

	local PlayerPrefs = UE.PlayerPrefs
	local joSetting = PlayerPrefs.GetString("SystemSettings")
	SystemSettings = #joSetting > 0 and cjson.decode(joSetting) or nil
	if SystemSettings then
		if SystemSettings.Audio == nil or SystemSettings.Pushing == nil then 
			SystemSettings = DefSettings
		end
	end

	if SystemSettings == nil then SystemSettings = DefSettings end

	-- Video
	local quality = SystemSettings.videoQuality
	Performance = PerformanceSets[quality]

	local resH = Performance.resH
	if resH == 0 then resH = nil end
	libngui.LimitResolution(resH)

	-- Audio
	local Audio = SystemSettings.Audio
	local audMgr = CS.AudioMgr.Singleton
	audMgr.mute = Audio.mute
	audMgr.Sound_mute = Audio.sound_mute
	audMgr.BGM_mute = Audio.bgm_mute

	print(" - SETTINGS loaded. - ")
end

function P.set_video(quality)
	local saved = SystemSettings.videoQuality
	if saved == nil or saved ~= quality then
		SystemSettings.dirty = true
		SystemSettings.videoQuality = quality
		Performance = PerformanceSets[quality]

		local resH = Performance.resH
		if resH == 0 then resH = nil end
		libngui.LimitResolution(resH)
	end
end

function P.set_audio(key, value)
	if SystemSettings.Audio[key] ~= value then
		SystemSettings.dirty = true
		SystemSettings.Audio[key] = value
	end
end

function P.set_push(id, value)
	if SystemSettings.Pushing[id] ~= value then
		SystemSettings.dirty = true
		SystemSettings.Pushing[id] = value
	end
end

function P.get_video(key)
	return Performance[key]
end

function P.get_audio(key)
	return SystemSettings.Audio[key]
end

function P.get_push(id)
	return SystemSettings.Pushing[id]
end

function P.save()
	if SystemSettings.dirty then
		SystemSettings.dirty = nil
		local PlayerPrefs = UE.PlayerPrefs
		PlayerPrefs.SetString("SystemSettings", cjson.encode(SystemSettings))
	end
end

return const(P)
