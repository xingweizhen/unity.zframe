-- File Name: framework/sdk.lua

-- 苹果内置支付支付返回结果
local function on_iap_purchased(Param)
	local appstoreId = Param.product
	local goods = UTIL.searchent(CFG_CHARGE_RELATION, { AppStoreGoodsID = appstoreId })
	local id = tonumber(goods.GoodsID)
	
	local data = Param.data
	local nm = NW.msg("CS_PAY_PRO_ORDER", 10240)
	nm:writeU32(id)
	nm:writeString(data)
	NW.send(nm)

	-- 热云统计
	libsystem.ProcessingData(cjson.encode({
		method = "SubmitData",
		tag = "OnPay",
		Data = {
			order = Param.order,
			payment = "apple",
			currency = "CNY",
			amount = goods.Price and goods.Price / 100 or 0,
		},
	}))
end

local function on_iap_purchased_finish(Param)
	-- local order = Param.order
	-- if order then
	-- 	libanalytis.OnChargeSuccess(order)
	-- end
end

local function on_sdk_logined(Param)
	-- 防止苹果会自动登录gamecenter，所以不在登陆界面的话直接返回
	local Top = _G.PKG["ui/uimgr"].top()
	if Top == nil then return false end
	if Top.path ~= "UI/WNDLaunch" or not Top:is_opened() then return false end
	
	local Acc = LOGIN_MGR.new_acc(tostring(Param.uid), Param.token)
	Acc.name = Param.uname
	LOGIN_MGR.try_account_login()
end

local function on_sdk_login_fail(Param)
	
end

local function on_sdk_registed(Param)
	on_sdk_logined(Param)
end

local function on_sdk_purchased(Param)
end

local function on_sdk_logouted(Param)
	-- 登出
	DATA.do_logout()
end

local function on_sdk_exitgame(Param)
	libunity.AppQuit()
end

local function on_sdk_view_closed(Param)
	
end

local function on_goods_purchased(Param)
	-- 无论结果如何 关闭屏蔽界面
	_G.UI.Waiting.hide()
end

local FuncMap = {
	-- 苹果内置支付支付
	iap_purchased = on_iap_purchased,
	-- 苹果内置支付成功
	iap_purchased_finish = on_iap_purchased_finish,
	-- 平台登录
	sdk_logined = on_sdk_logined,
	-- 平台登录失败
	sdk_login_fail = on_sdk_login_fail,
	-- 平台注册
	sdk_registed = on_sdk_registed,
	-- 平台支付
	sdk_purchased = on_sdk_purchased,
	-- 平台登出
	sdk_logouted = on_sdk_logouted,
	-- 退出游戏
	sdk_exitgame = on_sdk_exitgame,
	-- 平台界面关闭
	sdk_view_closed = on_sdk_view_closed,
	-- 商品购买回调
	goods_purchased = on_goods_purchased,
}

local function on_sdk_message(param)
	local Param = cjson.decode(param)
	if Param then
		local cbf = FuncMap[Param.method]
		if cbf then cbf(Param) end
	end
end

return {
	sdk_message = on_sdk_message,
}
