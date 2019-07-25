--
-- @file    ui/window.lua
-- @author  xingweizhen
-- @date    2015-04-08 19:19:23
-- @desc    UI管理库
--

local ipairs, pairs, libunity, libugui
    = ipairs, pairs, libunity, libugui

-- 窗口导航栈
local WNDStack = _G.DEF.Stack.new()

-- 已打开的窗口和导航栈中的窗口
local LCWnds = {}

local ui = {
	DEPTH_FRM = 1,
	DEPTH_WND = 2,
}
rawset(_G, "ui", ui)
-- ui库hook方法集
local Hooks = {}
local function callhook(k, ...)
	local f = Hooks[k]
	if f then
		local status, ret = trycall(f, ...)
		if status then return ret end
	else return true end
end

local RefDEF = {}
RefDEF.__index = function (t, k)
	if k == nil then return end

	local go, val = rawget(t, "go")
	local preffix = k:sub(1, 2)
	if preffix == "lb" or preffix == "sp" then
		val = libugui.FindGraphic(go, k)
	else
		-- 长度大于3才会查找子对象，否则从自身上查找组件
		if #k > 3 then
			go = libunity.Find(go, k)
			if go == nil then return end

			local preffix = k:sub(1, 3)
			if preffix == "Grp" then
				val = setmetatable({ go = go, grp = libugui.InitGroup(go), }, RefDEF)
				local entGo = libunity.Find(go, 0)
				if entGo then
					entGo:SetActive(false)
					val.Ent = setmetatable({ go = entGo }, RefDEF)
				end
			elseif preffix == "Sub" then
				val = setmetatable({ go = go, }, RefDEF)
			elseif preffix == "Elm" then
				val = go 
			end
		end

		if val == nil then
			-- 查找<IEventSystemHandler>，尽管对命名无要求，还是需要尽量遵循以下规则
			-- 		btn : UIButton
			-- 		tgl : UIToggle
			-- 		evt : UIEventTrigger
			-- 		inp : InputField
			-- 		bar : Slider or UIProgress or ScrollBar
			-- 		scr : ScrollRect
			val = libugui.FindEventHandler(go)
		end
	end

	if val then t[k] = val end
	return val
end

local OBJDEF = {
	_G = _G,
	libunity = libunity,
	libugui = libugui,
	libasset = libasset,
	libsystem = libsystem,
	libdata = libdata,
	libtimer = libtimer,
	libscene = libscene,
	libui = libui, libnet = libnet,
	ui = ui, cjson = cjson,
	AUD = AUD,

	--setfenv = setfenv, getfenv = getfenv,
	assert = assert, error = error,
	getmetatable = getmetatable, setmetatable = setmetatable,
	ipairs = ipairs, pairs = pairs,
	load = load,
	select = select, next = next, trycall = trycall,
	require = require, unpack = table.unpack, rawequal = rawequal, rawget = rawget, rawset = rawset,
	type = type, tonumber = tonumber, tostring = tostring,
	print = print, printf = printf,

	coroutine = coroutine,
	math = math,
	string = string,
	table = table,
	debug = debug,
	package = package, io = io, os = os,

	typeof = typeof,

	CS = _G.CS, UE = _G.UE, UGUI = _G.UGUI, GO = _G.GO, CVar = _G.CVar,

	config = _G.config,
	cfgname = _G.cfgname,
	next_action = _G.next_action,
}
OBJDEF.__index = OBJDEF

OBJDEF.__tostring = function (self)
	if self then
		return string.format("[Window:%s@%d<%s>]", self.path, self.depth or 0, tostring(self:is_opened()))
	else
		return "[WindowDEF]"
	end
end

-- 所有UI逻辑代码的chunk
local function get_lcchunk(pkgName)
	if pkgName then
		local chunk = rawget(_G.PKG, pkgName)
		if chunk == nil then
			chunk = loadfile(pkgName)
			rawset(_G.PKG, pkgName, chunk)
		end
		return chunk
	end
end

local function set_blocksraycast(go, value)
	-- local cvGrp = go:GetComponent("CanvasGroup")
	-- if cvGrp then cvGrp.blocksRaycasts = value end
end

local function do_create_wnd(go)
	set_blocksraycast(go, true)

	local Wnd = LCWnds[go.name]
	if Wnd then
		if Wnd.show_view then Wnd.show_view() end
	else
		libunity.LogW("{0}的窗口类不存在", go)
	end
end

local function subscribe_wndmsg(Wnd)
	if Wnd.Handlers then
		local libnet = _G.libnet
		for k,v in pairs(Wnd.Handlers) do
			libnet.subscribe(k, v)
		end
	end
end

local function on_open(go, pkgName)
	set_blocksraycast(go, false)

	local lcName = go.name
	local Wnd = LCWnds[lcName]
	Wnd.go = go

	if rawget(Wnd, "Ref") == nil then
		local chunk = get_lcchunk(pkgName)
		if chunk == nil then 
			libunity.LogE("窗口{0}对应的lua脚本[{1}]不存在", Wnd, pkgName) 
			return
		end

		local stackIdx, _ = table.ifind(WNDStack, Wnd)
		local Context = Wnd.Context
		Wnd.__index = Wnd
		Wnd = setmetatable(chunk(), Wnd)
		Wnd.Context = Context
		LCWnds[lcName] = Wnd

		Wnd.Ref = ui.ref(go)
		subscribe_wndmsg(Wnd)

		trycall(Wnd.init_view)
		callhook("awaking", Wnd)
		if stackIdx then WNDStack[stackIdx] = Wnd end
	else
		subscribe_wndmsg(Wnd)
	end

	trycall(Wnd.init_logic)
	callhook("starting", Wnd)

	if Wnd.instantly then
		libunity.Invoke(go, 0, do_create_wnd, go)
	else
		libugui.DOTween(go, 1, do_create_wnd, true)
	end
end

local function on_close(go, pkgName)
	local lcName = go.name
	local Wnd = LCWnds[lcName]
	if Wnd == nil then
		libunity.LogE("{0} is nil.", lcName)
	return end

	callhook("closing", Wnd)

	trycall(Wnd.on_recycle)

	-- 取消订阅网络消息
	Wnd:unsubscribe()

	-- 窗口不在栈中才移除引用
	local stackIdx, _ = table.ifind(WNDStack, Wnd)
	if stackIdx == nil then
		LCWnds[lcName] = nil
	end
end

local function on_event(lcName, event, ...)
	local Wnd = LCWnds[lcName]
	if Wnd and Wnd.go then
		local handler = rawget(Wnd, event)
		if handler then
			handler(...)
		else
			libunity.LogW("{0} missing handler for [{1}]", lcName, event)
		end
	end
end

local function u_create(prefab, depth)
	local GOType = typeof(UE.GameObject)
	if type(prefab) == "string" then
		prefab = libasset.Load(GOType, prefab)
	else
		libunity.SetActive(prefab, true)
	end

	return ui.create(prefab, depth)
end

local function u_close(go, instantly, stacked)
	if go then
		set_blocksraycast(go, false)

		if instantly then
			libunity.Recycle(go)
		else
			libugui.DOTween(go, -1, libunity.Recycle, false)
		end
	end
end

local function close_window(self, instantly, stacked)
	-- if self.locked then return end
	self.locked = true

	if self.closing then self.closing() end
	u_close(self.go, instantly, stacked)
	if not stacked then self.go = nil end
end

--=============================================================================

function OBJDEF.new(path, depth, stacked)
	local self = {
		path = path,
		name = path:getfile(),
		depth = depth,
		stacked = stacked,
		__tostring = OBJDEF.__tostring,
   	}
   	setmetatable(self, OBJDEF)

   	LCWnds[self.name] = self
   	return self
end

function OBJDEF:find_sub_wnd(wndName)
	return self.SubWnd and self.SubWnd[wndName]
end

function OBJDEF:create_sub_wnd(wndName, depth, Context)
	self.SubWnd = table.need(self, "SubWnd")
	if self.SubWnd[wndName] then
		return self.SubWnd[wndName]
	end
	local Sub = ui.ref(ui.create("UI/"..wndName, depth))
	Sub.Context = Context
	self.SubWnd[wndName] = Sub

	return Sub
end

function OBJDEF:close_sub_wnd(wndName)
	if self.SubWnd and self.SubWnd[wndName] then
		libugui.CloseWindow(wndName)
		self.SubWnd[wndName] = nil
	end
end

function OBJDEF:is_opened()
	return libunity.IsActive(self.go, true, true)
end

function OBJDEF:open(instantly)
	self.locked = nil

	if self.Cached then
		for _,Data in ipairs(self.Cached) do
			Data.T[Data.k] = Data.v
		end
		self.Cached = nil
	end
	if self.stacked then self.action = "push" end
	self.go = u_create(self.path, self.depth)
	self.instantly = instantly
	if self.opening then self.opening() end
end

-- 关闭窗口，并把窗口从栈中移除（如果在）
function OBJDEF:remove(instantly)
	local stackIdx, _ = table.ifind(WNDStack, self)
	if stackIdx then table.remove(WNDStack, stackIdx) end
	close_window(self, instantly, false)
end

-- 关闭窗口，并弹出栈内的下一个窗口
function OBJDEF:close(instantly)
	-- 避免反复调关闭
	if self.locked then return end
	self.locked = true
	self.instantly = instantly

	if self.SubWnd then
		for wndName,_ in pairs(self.SubWnd) do
			self:close_sub_wnd(wndName)
		end
	end

	if self.stacked then
		self.action = "pop"
		self:remove(instantly)

		local TopWnd, n = WNDStack:peek(0), 0
		if TopWnd then
			while TopWnd do
				callhook("pop", self, TopWnd)

				if not TopWnd:is_opened() then TopWnd:open(TopWnd.depth > ui.DEPTH_WND) end
				if TopWnd.depth <= ui.DEPTH_WND then break end
				n = n + 1
				TopWnd = WNDStack:peek(n)
			end
		else
			callhook("pop", self)
		end
	else
		close_window(self, instantly, false)
	end
end

function OBJDEF:set_open(cbf)
	self.opening = cbf
end

function OBJDEF:set_close(cbf)
	self.closing = cbf
end

function OBJDEF:cache_data(Table, key)
	local Cached = table.need(self, "Cached")
	table.insert(Cached, { T = Table, k = key, v = Table[key] })
end

-- 动态订阅消息，只有在窗口有效的情况下生效（本地数据模式忽略此订阅）
function OBJDEF:subscribe(code, handler, replace)
	if rawget(self, "Ref") then
		local libnet = _G.libnet
		if libnet.enabled() then
			local Handlers = table.need(self, "Handlers")
			local oldhandler, newhandler = Handlers[code], handler
			local hType = type(oldhandler)
			if hType == "function" then
				-- 表结构
				if replace then
					libnet.unsubscribe(code, oldhandler)
				else
					newhandler = { oldhandler, handler }
				end
			elseif hType == "table" then
				if replace then
					libnet.unsubscribe(code, oldhandler)
				else
					table.insert(oldhandler, handler)
					newhandler = oldhandler
				end
			end

			Handlers[code] = newhandler
			libnet.subscribe(code, handler)
		end
	end
end

-- 主动取消所有消息订阅
function OBJDEF:unsubscribe(code)
	local Handlers = rawget(self, "Handlers")
	if Handlers then
		local libnet = _G.libnet

		if code then
			local handler = Handlers[code]
			if handler then
				libnet.unsubscribe(code, handler)
				Handlers[code] = nil
			end
		else
			for k,v in pairs(Handlers) do
				libnet.unsubscribe(k, v)
			end
		end
	end
end
--=============================================================================

--
-- 下面这些是Group界面容器的生成方法
-- @ Grp是容器的表，初始值
-- 		{ go = <GameObject>, Ent = { go = <GameObject>, ... }, }
-- @ reg是为容器内每个控件的触发函数赋值的函数
--

local GroupDEF = {}
GroupDEF.__index = GroupDEF

do
	-- 生成模板Ent
	function GroupDEF:init(prefab, entName)
		if entName == nil then entName = "ent" end
		local ent = libunity.Find(self.go, entName)
		if ent == nil then
			if type(prefab) == "string" then
				prefab = "UI/"..prefab
			end
			ent = libunity.NewChild(self.go, prefab, entName)
		end
		local Ent = ui.ref(ent)
		if self.reg then self.reg(nil, Ent) end
		libunity.SetActive(ent, false)
		self.Ent = Ent
		return Ent
	end

	-- 根据【数据索引】查找一个Ent
	function GroupDEF:find(index)
		local go = self.grp:Find(index)
		if go then return ui.ref(go) end
	end

	-- 设置一个Ent的【数据索引】，参数go必须是Ent的gameObject对象
	function GroupDEF:setindex(go, index)
		self.grp:SetIndex(go, index)
	end

	-- 获取一个Ent的【数据索引】，参数go必须是Ent的gameObject对象
	function GroupDEF:getindex(go)
		local index = self.grp:GetIndex(go)
		return index >= 0 and index or nil
	end

	-- 查找一个【对象索引】为i的Ent
	function GroupDEF:get(i)
		local go = self.grp:Get(i - 1)
		if go then return ui.ref(go) end
	end

	-- 查找一个【对象索引】为i的Ent，如果不存在则自动生成一个，并设置其【数据索引】为i
	function GroupDEF:gen(i)
		local Ent, isNew = self:get(i), false
		if Ent == nil then
			local ent = self.Ent.go
			local go = libunity.NewChild(self.go, ent, ent.name..i)
			self.grp:Add(go, i)
			libunity.SetActive(go, true)
			Ent = ui.ref(go)
			if self.reg then self.reg(Ent, self.Ent) end
			isNew = true
		else
			libunity.SetActive(Ent.go, true)
		end
		return Ent, isNew
	end

	-- 生成并初始化n个Ent（【对象索引】从1~n)，超过n的Ent会自动被隐藏
	function GroupDEF:dup(n, cbf)
		for i=1,n do
			local Ent, isNew = self:gen(i)
			if cbf then cbf(i, Ent, isNew) end
		end

		local i = n
		while true do
			local go = self.grp:Get(i)
			if go then
				libunity.SetActive(go, false)
				i = i + 1
			else break end
		end
	end

	-- 通过【对象索引】遍历所有存在的Ent(从1~max)
	function GroupDEF:pairs(activeOnly)
		local i = 0
		if activeOnly then
			return function ()
				while true do
					local go = self.grp:Get(i); i = i + 1
					if go == nil then return end
					if go.activeSelf then
						return i, ui.ref(go)
					end
				end
			end
		else
			return function ()
				i = i + 1
				local v = self:get(i)
				if v then return i, v else return nil, nil end
			end
		end
	end

	-- 通过【对象索引】遍历所有存在的gameObject(从1~max)
	function GroupDEF:ipairs(activeOnly)
		local i = 0
		if activeOnly then
			return function ()
				while true do
					local go = self.grp:Get(i); i = i + 1
					if go == nil then return end
					if go.activeSelf then return i, go end
				end
			end
		else
			return function ()
				local go = self.grp:Get(i); i = i + 1
				if go then return i, go end
			end
		end
	end

	-- 使用一个数组的数据进行初始化，不会生成新的Ent
	function GroupDEF:view(List, cbf)
		for i,v in self:pairs() do
			local Obj = List[i]
			if Obj then
				libunity.SetActive(v.go, true)
				cbf(Obj, v)
			else
				libunity.SetActive(v.go, false)
			end
		end
	end

	-- 通过【对象索引】设置一个Ent是否Active
	function GroupDEF:set_active(i, active)
		local n = 0
		if i then
			libunity.SetActive(self.grp:Get(i - 1), active)
		else
			for _, go in self:ipairs(true) do
				libunity.SetActive(go, active)
			end
		end
		return n
	end

	-- 通过【数据索引】设置一个Ent是否Active
	function GroupDEF:nset_active(n, active)
		local go = self.grp:Find(n)
		if go then libunity.SetActive(go, active) end
	end

	-- 显示所有Ent
	function GroupDEF:show()
		return self:set_active(nil, true)
	end

	-- 隐藏所有Ent
	function GroupDEF:hide()
		return self:set_active(nil, false)
	end

	-- 仅显示特定数量的Ent
	function GroupDEF:limit(n)
		for i, go in self:ipairs() do
			libunity.SetActive(go, i <= n)
		end
	end

	function GroupDEF:combine(Ent, prefab, subName)
		local Sub = Ent[subName]
		if Sub == nil then
	       local go = libunity.NewChild(Ent.go, prefab, subName)
			libunity.SetEnable(GO(go, nil, "Collider"), false)
        	Sub = ui.ref(go)
        	Ent[subName] = Sub
        end
        Sub.__index = Sub
        setmetatable(Ent, Sub)
	end

	-- 生成和合并一个Ent
	function GroupDEF:gen_combine(i, prefab, subName)
		if subName == nil then subName = "Sub" end
	    local Ent, isNew = self:gen(i)
	    self:combine(Ent, "UI/"..prefab, subName)
	    return Ent, isNew
	end

	-- 生成和合并并初始化多个Ent
	function GroupDEF:dup_combine(n, prefab, cbf, subName)
		if subName == nil then subName = "Sub" end
	    local prefab = libasset.Load(typeof(UE.GameObject), "UI/"..prefab)
	    self:dup(n, function (i, Ent, isNew)
	    	self:combine(Ent, prefab, subName)
	        if cbf then cbf(i, Ent, isNew) end
	    end)
	end
end
-- 完成"UI组"类的定义 @

--=============================================================================

function ui.hook(event, func)
	Hooks[event] = func
end

function ui.new()
	return setmetatable({}, OBJDEF)
end

function ui.create(prefab, depth, canvas)
	return libugui.CreateWindow(prefab, depth or 0, canvas)
end

-- 获取栈顶的窗口
function ui.peek(n)
	return WNDStack:peek(n)
end

-- 打开一个弹窗
-- @path 	窗口预设路径
-- @depth 	窗口层级
-- @Context 窗口上下文数据
function ui.show(path, depth, Context, instantly)
	local lcName = path:getfile()
	local Wnd = LCWnds[lcName]
	if Wnd then return Wnd end
	if not callhook("willopen", lcName) then return end

	Wnd = OBJDEF.new(path, depth)
	Wnd.Context = Context
	Wnd:open(instantly)

	return Wnd
end

-- 创建一个窗口，并压入窗口栈
-- @path 	窗口预设路径
-- @depth 	窗口层级
-- @Context 窗口上下文数据
function ui.open(path, depth, Context, instantly)
	local lcName = path:getfile()

	local Wnd = LCWnds[lcName]
	if Wnd then return Wnd end
	if not callhook("willopen", lcName) then return end

	local NewWnd = LCWnds[lcName]
	if NewWnd then
		if NewWnd:is_opened() then
			libunity.LogW("窗口[{0}]已经被打开", path)
		else
			NewWnd:open()
		end
		NewWnd.Context = Context
		return NewWnd
	end

	if depth == nil or depth == 0 then depth = ui.DEPTH_WND end
	-- 记录因本窗口打开而关闭的窗口数量
	if depth > ui.DEPTH_WND then
		-- 非独占界面
		local TopWnd = WNDStack:peek()
		if TopWnd then
			-- 栈顶窗口的深度如果相同就关闭
			if TopWnd.depth == depth then
				close_window(TopWnd, true, true)
			end
		end
	else
		-- 独占界面，关闭栈中所有可见的界面
		local TopWnd, n = WNDStack:peek(0), 0
		while TopWnd do
			close_window(TopWnd, nil, true)
			if TopWnd.depth <= ui.DEPTH_WND then break end
			n = n + 1
			TopWnd = WNDStack:peek(n)
		end
	end

	NewWnd = OBJDEF.new(path, depth, true)
	NewWnd.Context = Context
	WNDStack:push(NewWnd)
	NewWnd:open(instantly)
	return NewWnd
end

-- 查找一个窗口
function ui.find(name, includeStacked)
	local Wnd = LCWnds[name]
	if Wnd and (not includeStacked or Wnd:is_opened()) then
		return Wnd
	end
end

function ui.set_visible(Wnd, visible)
	if type(Wnd) == "string" then
		Wnd = ui.find(Wnd)
	end

	if Wnd then
		if Wnd.set_visible then
			Wnd.set_visible(visible)
		else
			libugui.SetVisible(Wnd.go, visible)
		end
	end
end

-- 关闭一个窗口
function ui.close(name, instantly)
	local nameType = type(name)
	if nameType == "string" then
		local Wnd = ui.find(name)
		if Wnd then Wnd:close(instantly) end
	elseif nameType == "table" then
		-- 调用原始的关闭方法
		OBJDEF.close(name, instantly)
	else
		libunity.SetActive(name, true)
		libunity.Recycle(name)
	end
end

function ui.clear_stack()
	for k,v in pairs(LCWnds) do
		if v.stacked then LCWnds[k] = nil end
	end
	WNDStack:clear()
end

function ui.foreach_lcwnds()
	return pairs(LCWnds)
end

-- 生成组函数（自动），保存控件的生成方法在容器表内
local function make_group(Grp, reg)
	Grp.reg = reg
	return setmetatable(Grp, GroupDEF)
end

-- 生成组函数（手动），保存控件的生成方法在容器表内
local function complete_group(prefab, Grp, reg, entName)
	make_group(Grp, reg)
	-- 生成一个初始Ent
	if prefab then Grp:init(prefab, entName) end
	return Grp
end

function ui.group(...)
	local arg = select(1, ...)
	if type(arg) == "table" then
		return make_group(...)
	else
		return complete_group(...)
	end
end

function ui.ref(go)
	return setmetatable({ go = go }, RefDEF)
end

-- 动态创建一个UI控件
function ui.gen(prefab, Root, subName)
    if subName == nil then subName = "Sub" end

    local Sub = Root[subName]
    if Sub == nil then
        local root = Root.go
        local go = libunity.NewChild(root, "UI/"..prefab, subName)
        Sub = ui.ref(go)
        Root[subName] = Sub
    end
    return Sub
end

-- 通过ent对象或者其内部对象[获取/设置]它的索引
function ui.index(uObj, index)
	if index then
		libugui.SetGroupIdx(uObj, index)
	else
		return libugui.GetGroupIdx(uObj)
	end
end

local CanvasName = {
	[0] = "UICanvas", [1] = "LayCanvas", [2] = "BackCanvas",
}
function ui.moveout(uobj, cv)
	libunity.SetActive(uobj, true)
	libunity.SetParent(uobj, "/UIROOT/" .. CanvasName[cv])
end
function ui.putback(uobj, go)
	libunity.SetParent(uobj, go)
	libunity.SetActive(uobj, false)
end

function ui.seticon(sp, path, clip)
	if sp == nil then return end

	sp:SetSprite(path)
	if clip and path and #path > 0 then
		if #clip > 0 then
			local dir = path:getdir()
			if dir then
				local matPath = string.format("Atlas/%s%s", dir, clip)
				sp.material = libasset.Load(nil, matPath)
			else
				sp.material = nil
				libunity.LogW("错误的图标路径：{0}", path)
			end
		else
			sp.material = nil
		end
	end
end

function ui.setenv(key, value)
	rawset(OBJDEF, key, value)
	rawset(_G, key, value)
end

function ui.setloc(preLang, lang)
	libugui.SetLocalize(lang, ui.defLang or "en")
	rawset(_G, "lang", lang)
end

return {
	 on_open = on_open,
	 on_close = on_close,
	 on_event = on_event,
}
