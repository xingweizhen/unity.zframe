-- File Name : framework/notify.lua

local FocusEvents = {}
local PushNotificationList = {}
local pausedTime

local function auto_clean_invalid_push_ntf()
	local nowTime = os.date2secs()
	local idx = 0
	for i=1,#PushNotificationList do
		idx = i
		if PushNotificationList[i].expiration > nowTime then
			idx = idx - 1
			break
		end
	end
	for i=idx,1,-1 do
        table.remove(PushNotificationList, i)
    end
end

local function remove_push_notification_by_name(name)
	for i=#PushNotificationList,1,-1 do
	local v = PushNotificationList[i]
		if name == v.name then
			table.remove(PushNotificationList, i)
		end
	end
end

local function register_push_notification()
	_G.SDK_KG.CleanAllNotificaiton()
	auto_clean_invalid_push_ntf()
	local nowTime = os.date2secs()
	local Settings = _G.Prefs.Settings:load()

	local PushSettingMap = {}

	for _,v in ipairs(CVar.PushSetting.AlwaysPush.TypeList) do
		PushSettingMap[v] = true
	end

	for _,v in ipairs(CVar.PushSetting) do
		local isPush = Settings[v.setting]
		if isPush == nil then isPush = true end
		for _,v1 in ipairs(v.TypeList) do
			PushSettingMap[v1] = isPush
		end
	end

	for _,v in ipairs(PushNotificationList) do
		if PushSettingMap[v.name] then
			local delaySeconds = v.expiration - nowTime
			_G.SDK_KG.DisplayLocalNotification(v.name, v.title, v.message, v.data, v.audioFileName, delaySeconds)
		end
	end
end

local function send_pause_switch(paused)
	pausedTime = pausedTime or 0
	if paused then
		pausedTime = UE.Time.realtimeSinceStartup
	elseif (pausedTime + 60) < UE.Time.realtimeSinceStartup then
		libnet.disconnect(nil, true)
		return
	end
	local nm = libnet.msg("LOGIN.CS.SWITCH_NOHUP")
	nm:writeU32(paused and 1 or 2)
	libnet.MainCli:send(nm)
end

local function on_app_pause(paused)
	print("on_app_pause", paused)

	local isShowAIHelp = rawget(_G.SDK_KG, "isShowAIHelp")
	if paused then
		register_push_notification()
		if isShowAIHelp then
			-- 如是切到AIHelp，则只发送LOGIN.CS.SUSPEND_HEARTBEAT
			local nm = libnet.msg("LOGIN.CS.SUSPEND_HEARTBEAT")
			libnet.send(nm)
			return
		end
	else
		-- 从AIHelp切回
		if isShowAIHelp then
			_G.SDK_KG.isShowAIHelp = nil
			_G.SDK_KG.bAiHelpAlert = nil
			libnet.broadcast("CLIENT.SC.SDK_AIHELP_ALERT")
		end

		_G.SDK_KG.CleanAllNotificaiton()
	end

	send_pause_switch(paused)
end

local function on_app_focus(focus)
	print("on_app_focus", focus)
	for _,event in pairs(FocusEvents) do event(focus) end
	_G.SDK_KG.ApplicationFocus(focus)
end

local function on_alert_click(msg)
	-- print("on_alert_click", msg)
	local AlertCBF = DY_DATA.AlertCBF
	local cbf = AlertCBF[msg]
	if cbf then
		cbf()
		AlertCBF[msg] = nil
	end
end

-- 收到内存警告
local function on_mem_warning(msg)

end

local function subscribe_focus(key, value)
	FocusEvents[key] = value
end

local function unsubscribe_focus(key)
	FocusEvents[key] = nil
end

-- 新增推送提示
local function push_notification(name, title, message, data, audioFileName, delaySeconds, replaceName)
	auto_clean_invalid_push_ntf()
	if replaceName then remove_push_notification_by_name(name) end

	if delaySeconds < 0 then
		return
	end

	local expiration = os.date2secs() + delaySeconds
	local idx = 1
	for i=#PushNotificationList,1,-1 do
		local v = PushNotificationList[i]
		if expiration > v.expiration then
			idx = i + 1
			break
		end
	end

	table.insert(PushNotificationList, idx, {
		name = name,
		title = title,
		message = message,
		data = data,
		audioFileName = audioFileName,
		expiration = expiration,
	})

	--register_push_notification()
end

return {
	on_app_pause = on_app_pause,
	on_app_focus = on_app_focus,
	on_alert_click = on_alert_click,
	on_mem_warning = on_mem_warning,
	subscribe_focus = subscribe_focus,
	unsubscribe_focus = unsubscribe_focus,
	push_notification = push_notification,

	register_push_notification = register_push_notification,
}
