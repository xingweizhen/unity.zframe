-- File Name : framework/ui/messagebox.lua

-- 消息框用法
-- 同时只会显示一个消息框，其他的消息框进入队列，当前一个关闭后，弹出下一个。

-- libui.MBox.make("MBNormal")
--    :set_param("content", content)
--    :show()

local libunity = require "libunity.cs"
local BoxQueue, ActivatedBox = _G.DEF.Queue.new(), nil

--=============================================================================

local OBJDEF = {}
OBJDEF.__index = OBJDEF
OBJDEF.__tostring = function (self)
    return string.format("[MB:%s@%s]", self.prefab, tostring(self.depth))
end

local function do_popup_box()
    local function close_instanly()
        if ActivatedBox and ActivatedBox.Wnd then
            ActivatedBox.Wnd:close(true)
        end
    end

    if OBJDEF.blocking then
        close_instanly()

        OBJDEF.blocking = nil
        ActivatedBox = OBJDEF
    return end

    local Box = BoxQueue:dequeue()
    if Box then
        close_instanly()

        ActivatedBox = Box
        local Wnd = ui.show("UI/"..Box.prefab, Box.depth, Box.Params)
        ActivatedBox.Wnd = Wnd
    else
        if ActivatedBox and ActivatedBox.Wnd then
            ActivatedBox.Wnd:close()
        end
        ActivatedBox = nil

        if OBJDEF.on_empty then
            OBJDEF.on_empty()
            OBJDEF.on_empty = nil
        end
    end
end

OBJDEF.close = do_popup_box

function OBJDEF.on_btnaction(button)
    if ActivatedBox then
        local onaction = ActivatedBox["on_" .. button]
        if onaction then onaction() end

        if ActivatedBox.Wnd then
            local nmcode = ActivatedBox["nm_" .. button]
            local cbf = ActivatedBox["cbf_" .. button]
            if nmcode then
                local Wnd = ui.find(ActivatedBox.prefab)
                if Wnd then
                    Wnd:subscribe(nmcode, function (Ret)
                        if cbf then cbf(Ret) end
                        if Ret.err == nil then
                            Wnd:unsubscribe(nmcode)
                            do_popup_box()
                        end
                    end)
                end
            return end
        end
    end
    do_popup_box()
end

function OBJDEF.on_btnconfirm_click()
    OBJDEF.on_btnaction("confirm")
end

function OBJDEF.on_btncancel_click()
    OBJDEF.on_btnaction("cancel")
end

function OBJDEF.get() return ActivatedBox end

function OBJDEF:set_depth(depth)
    self.depth = depth
    return self
end

function OBJDEF:set_event(on_confirm, on_cancel)
    if on_confirm then self.on_confirm = on_confirm end
    if on_cancel then self.on_cancel = on_cancel end
    return self
end

function OBJDEF:set_action(action, callback)
    self["on_" .. action] = callback
    return self
end

-- 设置确定或取消操作执行前需要等待的网络消息和回调
function OBJDEF:set_handler(action, nmmsg, callback)
    if nmmsg then
        self["nm_" .. action] = nmmsg
        self["cbf_"..action] = callback
    end
end

function OBJDEF:set_param(key, value)
    if value then
        self.Params[key] = value
    end
    return self
end

function OBJDEF:set_params(Params)
    if Params then
        for k,v in pairs(Params) do self:set_param(k, v) end
    end
    return self
end

-- 设置标志后，不会有新的Box加入队列
function OBJDEF:as_final()
    self.final = true
    return self
end

function OBJDEF:show()
    if ActivatedBox == nil or ActivatedBox == self or
        ActivatedBox.Wnd == nil then
        ActivatedBox = nil
        do_popup_box()
    else
        --print("activated = ", ActivatedBox)
    end
    return self
end

-- 设置所有Box都弹出之后的动作
function OBJDEF.set_empty(action)
    OBJDEF.on_empty = action
end

function OBJDEF.block()
    if ActivatedBox == nil then
        ActivatedBox = OBJDEF
    else
        OBJDEF.blocking = true
    end
end

function OBJDEF.clear()
    BoxQueue:clear()
    OBJDEF.on_empty = nil
    OBJDEF.blocking = nil
    if ActivatedBox then OBJDEF.close() end
end

function OBJDEF.is_queued(Params)
    for _,Box in ipairs(BoxQueue) do
        local match = true
        for k,v in pairs(Params) do
            if Box.Params[k] ~= v then
                match = false
            break end
        end
        if match then return true end
    end
end

function OBJDEF.is_active(Params)
    if ActivatedBox and ActivatedBox.Wnd then
        if Params then
            for k,v in pairs(Params) do
                if ActivatedBox.Params[k] ~= v then return false end
            end
        end
        return true
    end
    return false
end

setmetatable(OBJDEF, { __call = function (_, prefab)
    if prefab == nil then prefab = "MBNormal" end
    local self = { prefab = prefab, Params = {} }
    setmetatable(self, OBJDEF)
    if ActivatedBox == nil or not ActivatedBox.final then
        BoxQueue:enqueue(self)
    end
    return self
end})

function OBJDEF.exception(onCancel)
    local TEXT = _G.TEXT
    OBJDEF()
        :set_param("content", onCancel and TEXT.tipExceptionRetry or TEXT.tipExceptionQuit)
        :set_param("single", onCancel == nil)
        :set_param("block", true)
        :set_param("txtConfirm", TEXT.btnQuit)
        :set_param("txtCancel", TEXT.btnRetry)
        :set_event(libunity.AppQuit, onCancel)
        :as_final():set_depth(100)
        :show()
end

-- =======================================
-- TODO: 下面的方法和业务逻辑有关了，应该移出框架
-- =======================================

function OBJDEF.legacy(content, onConfirm, cancel, onCancel)
    if cancel == nil then cancel = true end
    OBJDEF("MBNormal")
        :set_param("content", content)
        :set_param("single", not cancel)
        :set_param("block", not cancel)
        :set_event(onConfirm, onCancel)
        :show()
end
function OBJDEF.general(operateData, Params, onConfirm, onCancel)
    OBJDEF("MBNormal")
        :set_param("title", operateData.title)
        :set_param("content", operateData.content)
        :set_param("single", operateData.single)
        :set_param("block", operateData.block)
        :set_params(Params)
        :set_event(onConfirm, onCancel)
        :show()
end
-- 通用奖励弹窗
function OBJDEF.reward(title, Rewards, Params, onCancel)
    OBJDEF("WNDReward")
        :set_instant(true)
        :set_param("title", title)
        :set_param("Rewards", Rewards)
        :set_params(Params)
        :set_event(onCancel, onCancel)
        :show()
    libunity.PlaySound("Sound/common_complet")
end

-- 通用操作询问弹窗
function OBJDEF.operate(operateType, action, Params, throwBox)
    local OperText = operateType and TEXT.AskOperation[operateType]
    local Box = OBJDEF("MBNormal")
    if OperText then
        Box:set_param("title", OperText.title)
           :set_param("content", OperText.content)
           :set_param("txtConfirm", OperText.btnConfirm)
           :set_param("txtCancel", OperText.btnCancel)
    end
    local opBox = Box:set_params(Params):set_event(action)
    if throwBox then
        return opBox
    else
        opBox:show()
    end
end

-- 通用操作询问弹窗（底部）
function OBJDEF.operate_bottom(operateType, onConfirm, onCancel)
    local OperText = TEXT.AskOperation[operateType]
    return OBJDEF("MBBottom")
        :set_param("title" , OperText.title)
        :set_param("content", OperText.content)
        :set_param("txtConfirm", OperText.btnConfirm)
        :set_param("txtCancel", OperText.btnCancel)
        :set_param("single" , not onCancel)
        :set_event(onConfirm, onCancel)
end

-- 通用操作询问弹窗（带图片）
function OBJDEF.operate_with_image(operateType, action, Params, throwBox)
	local OperText = operateType and TEXT.AskOperation[operateType]
    local Box = OBJDEF("MBNormalWithImage")
    if OperText then
        Box:set_param("title", OperText.title)
           :set_param("content", OperText.content)
		   :set_param("picture", OperText.picture)
           :set_param("txtConfirm", OperText.btnConfirm)
           :set_param("txtCancel", OperText.btnCancel)
           :set_param("limitBack",true)
    end
    local opBox = Box:set_params(Params):set_event(action)
    if throwBox then
        return opBox
    else
        opBox:show()
    end
end
-- 通用商城消费提示弹窗
function OBJDEF.recharge(Params, onConfirm, onCancel)
    local MBConsume = OBJDEF("MBGiftPackage")
            :set_params(Params)
            :set_event(onConfirm, onCancel)
            :show()
end
-- 通用获取道具提示弹窗
function OBJDEF.accept(Params, onConfirm, onCancel)
    local MBConsume = OBJDEF("MBAcceptTips")
            :set_params(Params)
            :set_event(onConfirm, onCancel)
            :show()
end
-- 通用消费询问弹窗
function OBJDEF.consume(Cost, consumeType, action, Params, confirmCbf)
    if Cost then
        local function check_action()
            if DY_DATA:check_item(Cost) then
                action()
            else
                if Params == nil then
                    Params = {}
                    Params.payCompleteCallback = function()
                        action()
                    end
                end
                if Cost.dat == 1 then
                    OBJDEF.buy_energy_alert(Params and Params.payCompleteCallback)
                elseif not OBJDEF.buy_normal_alert(Cost.dat, Params and Params.payCompleteCallback) then
                    local CostBase = Cost:get_base_data()
                    libui.Toast.norm(string.format(TEXT.fmtNotEnoughItem, CostBase.name))
                end
            end
        end
        if consumeType then
            local ConsumeText = TEXT.AskConsumption[consumeType]
            local MBConsume = OBJDEF("MBConsume")
                :set_param("title", ConsumeText.title)
                :set_param("oper", ConsumeText.oper)
                :set_param("tips", ConsumeText.tips)
                :set_param("Cost", Cost)
                :set_params(Params)
                :set_event(check_action)
                
            if Params and Params.nmmsg then
                if confirmCbf then MBConsume:set_handler("confirm", Params.nmmsg, confirmCbf) end
            end

            MBConsume:show()
        else
            check_action()
        end
    else action() end
end

function OBJDEF.consume_virtual_goods(virtualGoodsId, consumeType, Params, confirmCbf)
    if type(virtualGoodsId) == "string" then
        virtualGoodsId = CVar.VIRTUAL_GOODS[virtualGoodsId]
    end
    debug.printY("virtualGoodsId"..virtualGoodsId)

    local payUpperLimitText = Params.payUpperLimitText
    Params.shopType = CVar.SHOP_TYPE["VIRTUAL_SHOP"]
    Params.goodsId = virtualGoodsId

	local shopGoodsInfo = DY_DATA:get_shopgoods_info(
        Params.shopType, virtualGoodsId)
        
    debug.printG("shopGoodsInfo")
    debug.print(shopGoodsInfo)

	local payCnt = shopGoodsInfo.nPayCnt + 1
    local payLimitCnt = shopGoodsInfo.nPayLimitCnt
    
    if Params.lastCnt == nil then
        Params.lastCnt = payLimitCnt - shopGoodsInfo.nPayCnt
    end

    if Params.validityTime == nil then
        local shopInfo = DY_DATA.ShopInfo[Params.shopType]
        Params.validityTime = shopInfo.validityTime
    end

	if payLimitCnt == 0 or payCnt <= payLimitCnt then
        local Cost = _G.DEF.Item.new(shopGoodsInfo.assetType, shopGoodsInfo.curPrice)
        
        Params.nmmsg = "SHOP.SC.BUY_GOODS"
		OBJDEF.consume(Cost, consumeType, function ()
			libnet.SHOP.RequestBuyGoods(
				Params.shopType, virtualGoodsId)
		end, Params, confirmCbf)
    else
        -- 已达最大购买次数
        if payUpperLimitText then
            libui.Toast.norm(payUpperLimitText)
        end
	end
end

--=================================
-- 具体购买某种道具
--=================================
function OBJDEF.buy_energy_alert(payCompleteCallback)
	local operText = string.format(TEXT.AskConsumption.ResetEnergy.fmtOper, CVar.SHOP.BuyEnergyValue)
	local payUpperLimitText = TEXT.PayEnergyCntUpperLimit

	OBJDEF.consume_virtual_goods(CVar.VIRTUAL_GOODS["ENERGY"], "ResetEnergy", {
		oper = operText,
        payUpperLimitText = payUpperLimitText,
	}, payCompleteCallback)
end

function OBJDEF.buy_normal_alert(itemId, payCompleteCallback)
    local goodsId = config("paylib").get_paygoods_id(itemId)
    if goodsId then
        local itemName = config("itemlib").get_dat(itemId).name
        local operText = string.format(TEXT.AskConsumption.NormalBuyAltert.fmtOper, itemName)
        local payUpperLimitText = TEXT.fmtPayCntUpperLimit:csfmt(itemName)

        OBJDEF.consume_virtual_goods(goodsId, "NormalBuyAltert", {
            oper = operText,
            payUpperLimitText = payUpperLimitText,
        }, payCompleteCallback)

        return true
    end
end

function OBJDEF.item_received(Items)
    if Items then
        OBJDEF("MBItemReceived")
            :set_param("items", Items)
            :show()
    end
end
--=================================

_G.libui.MBox = OBJDEF
