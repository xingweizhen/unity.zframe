--
-- @file    framework/main.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    游戏Lua侧入口
--

print(_VERSION)
-- module --
dofile "framework/variable"

local function awake()
    print ("<color=yellow>lua awake</color>")
end

local function start()
    -- 读用户配置
    local UserSettings = {}
    local settingRoot = ENV.debug and "" or ENV.app_persistentdata_path .. "/"
    local f, err = io.open(settingRoot .. "user-settings.txt")
    if err == nil then
        for line in f:lines() do
            local l = line:trim()
            -- not starts with ‘#’
            if #l > 0 and l:byte(1) ~= 35 then
                local k, v = l:match("([%w_]+)=([^%c]+)")
                if k and v then UserSettings[k] = v end
            end
        end
        f:close()
    end

    rawset(_G, "UserSettings", UserSettings)

    dofile("game/init")
end

local function on_key(key)
    local ntf = libui.key_event(key)
    if ntf then ntf() end
end

local function on_ui_click(go)
    -- local name = go.name
    -- local collider = go.collider
    -- local pre = name:sub(1, 3)

end

-- 在最后，禁止定义和访问未定义的全局变量
setmetatable(_G, _G.MT.Const)

return {
    awake = awake,
    start = start,
    on_key = on_key,
    on_ui_click = on_ui_click,
}
