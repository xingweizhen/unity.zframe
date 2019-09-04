--
-- @file    framework/scenemgr.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

local P = {}

local function get_filelist_path()
	return string.format("%s/AssetBundles/filelist", _G.ENV.app_persistentdata_path)
end

local function load_file_list()
	if _G.ENV.using_assetbundle then
		local libcsharpio = require "libcsharpio.cs"
		local LFL = cjson.decode(libcsharpio.ReadAllText(get_filelist_path()))
		rawset(_G.ENV, "LFL", LFL)
	end
end

-- 各个场景加载前需要预载资源
local LevelAssetPreloads = { }

-- 各个场景加载完成后的回调
local LevelLoadedCbs = { }

-- 注册场景加载回调和预加载资源
function P.add_level(levelName, loadedCbf, preloader)
	LevelLoadedCbs[levelName] = loadedCbf
	LevelAssetPreloads[levelName] = preloader
end

-- 场景名称对应的标签
function P.level2tag(levelName)
	return levelName
end

-- 当场景加载完成时此函数会被调用。
-- 参数 @levelName	表示加载完成的场景名称
-- 参数 @launching 	表示该场景是否首个启动场景
function P.on_level_loaded(levelName, launching)
	-- ui.clear_stack()
	P.current = levelName
	print("level loaded", levelName)

	local levelTag = P.level2tag(levelName)
	local loadedCbf = LevelLoadedCbs[levelTag]

	if launching then
		-- 加载文件列表
		trycall(load_file_list)

		local launchCbf = LevelLoadedCbs[0]
		local PreloadAssets, launchingLoader, currentLoader = {}, LevelAssetPreloads[0], LevelAssetPreloads[levelTag]
		if launchingLoader then for i,v in ipairs(launchingLoader()) do table.insert(PreloadAssets, v) end end
		if currentLoader then for i,v in ipairs(currentLoader()) do table.insert(PreloadAssets, v) end end

		libasset.BatchLoadAsync(PreloadAssets, function ()
			if launchCbf then launchCbf() end
			if loadedCbf then loadedCbf(true) end
		end)
	else
		if loadedCbf then loadedCbf() end
	end
end

function P.load_level(levelTag, levelPath)
	local currentLoader = LevelAssetPreloads[levelTag]
	if currentLoader then
		libasset.PrepareAssets(currentLoader())
	end
	libunity.LoadLevel(levelPath, nil, P.on_level_loaded)
end

-- 资源下载消息
function P.on_asset_download_start(bundleName)
	-- body
end

function P.on_asset_download_error(bundleName, error)
	-- body
end

function P.on_asset_download_verifyfailed(bundleName, md5)
	-- body
end

function P.on_asset_downloaded(bundleName, siz)
	-- body
end

function P.on_asset_downloading(bundleName, current, total)
	-- body
end

return P
