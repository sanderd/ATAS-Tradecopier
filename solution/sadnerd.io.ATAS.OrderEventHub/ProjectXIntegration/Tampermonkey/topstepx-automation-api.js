// ==UserScript==
// @name         TopstepX Automation Hub
// @namespace    http://tampermonkey.net/
// @version      2025-03-19
// @description  try to take over the world!
// @author       You
// @match        https://topstepx.com
// @icon         https://www.google.com/s2/favicons?sz=64&domain=topstepx.com
// @grant        none
// @require      https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.js
// ==/UserScript==

(function() {
    'use strict';

function setReactInputValue(el, value) {
    // Store the last value to update React's internal tracker
    const last = el.value;
    // Use the native value setter to change the input's value
    const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
        window.HTMLInputElement.prototype,
        'value'
    ).set;
    nativeInputValueSetter.call(el, value);
    // Update React's internal value tracker
    const event = new Event('input', { bubbles: true });
    const tracker = el._valueTracker;
    if (tracker) {
        tracker.setValue(last);
    }
    // Dispatch the input event to notify React of the change
    el.dispatchEvent(event);
}

function setReactSelectValue(el, value) {
    // Store the last value to update React's internal tracker
    const last = el.value;
    // Use the native value setter to change the input's value
    const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
        window.HTMLSelectElement.prototype,
        'value'
    ).set;
    nativeInputValueSetter.call(el, value);
    // Update React's internal vtracker
    const event = new Event('change', { bubbles: true });
    const tracker = el._valueTracker;
    if (tracker) {
        tracker.setValue(last);
    }
    // Dispatch the input event to notify React of the change
    el.dispatchEvent(event);
}

const mouseClickEvents = ['mousedown', 'click', 'mouseup'];
function simulateMouseClick(element){
  mouseClickEvents.forEach(mouseEventType =>
    element.dispatchEvent(
      new MouseEvent(mouseEventType, {
          view: window,
          bubbles: true,
          cancelable: true,
          buttons: 1
      })
    )
  );
}

const mouseDblClickEvents = ['mousedown', 'dblclick', 'mouseup'];
function simulateMouseDoubleClick(element){
  mouseDblClickEvents.forEach(mouseEventType =>
    element.dispatchEvent(
      new MouseEvent(mouseEventType, {
          view: window,
          bubbles: true,
          cancelable: true,
          buttons: 1
      })
    )
  );
}

function waitForElm(selector) {
    return new Promise(resolve => {
        if (document.querySelector(selector)) {
            return resolve(document.querySelector(selector));
        }

        const observer = new MutationObserver(mutations => {
            if (document.querySelector(selector)) {
                observer.disconnect();
                resolve(document.querySelector(selector));
            }
        });

        // If you get "parameter 1 is not of type 'Node'" error, see https://stackoverflow.com/a/77855838/492336
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    });
}

function mapOrderFromOrderList(element) {
	var order = {};
	order.id = element.querySelector("div.MuiDataGrid-cell[data-field='id']").innerText;
	order.positionSize = element.querySelector("div.MuiDataGrid-cell[data-field='positionSize']").innerText;
	order.direction = element.querySelector("div.MuiDataGrid-cell[data-field='positionType']").innerText;
	order.orderType = element.querySelector("div.MuiDataGrid-cell[data-field='type']").innerText;
	order.limitPrice = element.querySelector("div.MuiDataGrid-cell[data-field='limitPrice']").innerText;
	order.stopPrice = element.querySelector("div.MuiDataGrid-cell[data-field='stopPrice']").innerText;
	order.executePrice = element.querySelector("div.MuiDataGrid-cell[data-field='executePrice']").innerText;
	order.status = element.querySelector("div.MuiDataGrid-cell[data-field='status']").innerText;

	// sanitize afterwards cuz lazy
	order.limitPrice = order.limitPrice ? parseFloat(order.limitPrice.replace(',', '')) : null;
	order.stopPrice = order.stopPrice ? parseFloat(order.stopPrice.replace(',', '')) : null;
	order.executePrice = order.executePrice ? parseFloat(order.executePrice.replace(',', '')) : null;
	order.positionSize = order.positionSize ? parseInt(order.positionSize) : null;

	return order;
}


function getMostRecentOrder(newerThanOrderId, criteria) {
	var orderTab = document.querySelector("div[id^='ordersTab-']");
	var sortOrder = orderTab.querySelector("div.MuiDataGrid-columnHeader[data-field='id'] svg[data-testid='ArrowDownwardIcon']");

	if(sortOrder === null) {
		alert('sort order of orders tab has to be ID desc');
		return null;
	}

	var orderRecords = orderTab.querySelectorAll(".MuiDataGrid-virtualScroller div[role='row']");
	for (let i = 0; i < orderRecords.length; i++) {
		var mapped = mapOrderFromOrderList(orderRecords[i]);

		if(newerThanOrderId && mapped.id == newerThanOrderId) return null;
		if(criteria && (criteria(mapped) === true)) return mapped;
		if(!criteria) return mapped;
	}

	return null;
}

let sleep = ms => new Promise(r => setTimeout(r, ms));
let waitFor = async function waitFor(f){
    while(!f()) await sleep(100);
    return f();
};
let waitUntilNotNull = async function waitUntilNotNull(f){
	let result = f();
	let i = 0;
    while(result === null && i < 10) {
		await sleep(100);
		result = f();
		//i++;
	}

    return result;
};

async function createLimitOrder(isLongPosition, price, contracts) {
	var mostRecentOrder = getMostRecentOrder();

	// open combobox order type
	simulateMouseClick(document.querySelector("div[class^='ordercard_order'] > div:nth-child(2) div[role='combobox']"));
	await waitForElm('.MuiPaper-root ul.MuiList-root');
	simulateMouseClick(document.querySelector("div[class^='MuiPaper-root'] ul li[data-value='1']"));
	await waitForElm("div[class^='ordercard_order'] > div:nth-child(3) input[type='number']");
	await waitForElm("div[class^='ordercard_order'] > div:nth-child(4) input[type='number']");

	var priceInput = document.querySelector("div[class^='ordercard_order'] > div:nth-child(3) input");
	var amountInput = document.querySelector("div[class^='ordercard_order'] > div:nth-child(4) input");

    priceInput[Object.keys(priceInput).filter((k) => k.startsWith('__reactProps'))[0]].onChange({'target' : { 'value' : price}});
	await sleep(10);
	priceInput[Object.keys(priceInput).filter((k) => k.startsWith('__reactProps'))[0]].onBlur(); // necessary or price will not update
	await sleep(10);
	amountInput.value = contracts;
    amountInput[Object.keys(amountInput).filter((k) => k.startsWith('__reactProps'))[0]].onChange({'target' : { 'value' : contracts}});
	await sleep(10);

	if(isLongPosition) {
		simulateMouseClick([...document.querySelectorAll("div[class^='ordercard_order'] button")].filter(a => a.textContent.includes("Buy"))[0])
	} else {
		simulateMouseClick([...document.querySelectorAll("div[class^='ordercard_order'] button")].filter(a => a.textContent.includes("Sell"))[0])
	}

	let createdOrder = await waitUntilNotNull(() => {
		return getMostRecentOrder(mostRecentOrder?.id, x => {
			return x.direction == (isLongPosition ? 'Buy' : 'Sell')
				&& x.positionSize == contracts
				&& (
					(x.orderType == 'Limit' && x.limitPrice == price)
					|| (x.orderType == 'Market' && x.executePrice !== null && (isLongPosition ? x.executePrice <= price : x.executePrice >= price))
				);
		});
	});

	return createdOrder;
}

async function createMarketOrder(isLongPosition, contracts) {
	var mostRecentOrder = getMostRecentOrder();

	// open combobox order type
	simulateMouseClick(document.querySelector("div[class^='ordercard_order'] > div:nth-child(2) div[role='combobox']"));
	await waitForElm('.MuiPaper-root ul.MuiList-root');
	simulateMouseClick(document.querySelector("div[class^='MuiPaper-root'] ul li[data-value='2']"));
	await waitForElm("div[class^='ordercard_order'] > div:nth-child(3) input[type='number']");

	var amountInput = document.querySelector("div[class^='ordercard_order'] > div:nth-child(3) input");
    amountInput.value = contracts;
    amountInput[Object.keys(amountInput).filter((k) => k.startsWith('__reactProps'))[0]].onChange({'target' : { 'value' : contracts}});
	await sleep(10);

	if(isLongPosition) {
		simulateMouseClick([...document.querySelectorAll("div[class^='ordercard_order'] button")].filter(a => a.textContent.includes("Buy"))[0])
	} else {
		simulateMouseClick([...document.querySelectorAll("div[class^='ordercard_order'] button")].filter(a => a.textContent.includes("Sell"))[0])
	}

	let createdOrder = await waitUntilNotNull(() => {
		return getMostRecentOrder(mostRecentOrder?.id, x => {
			return x.direction == (isLongPosition ? 'Buy' : 'Sell')
				&& x.positionSize == contracts
				&& x.orderType == 'Market'
				&& x.status == 'Filled';
		});
	});

	return createdOrder;
}

async function createStopOrder(isLongPosition, price, contracts) {
	var mostRecentOrder = getMostRecentOrder();

	// open combobox order type
	simulateMouseClick(document.querySelector("div[class^='ordercard_order'] > div:nth-child(2) div[role='combobox']"));
	await waitForElm('.MuiPaper-root ul.MuiList-root');
	simulateMouseClick(document.querySelector("div[class^='MuiPaper-root'] ul li[data-value='4']"));
	await waitForElm("div[class^='ordercard_order'] > div:nth-child(3) input[type='number']");
	await waitForElm("div[class^='ordercard_order'] > div:nth-child(4) input[type='number']");

	var priceInput = document.querySelector("div[class^='ordercard_order'] > div:nth-child(3) input");
	var amountInput = document.querySelector("div[class^='ordercard_order'] > div:nth-child(4) input");

    priceInput[Object.keys(priceInput).filter((k) => k.startsWith('__reactProps'))[0]].onChange({'target' : { 'value' : price}});
	await sleep(10);
	priceInput[Object.keys(priceInput).filter((k) => k.startsWith('__reactProps'))[0]].onBlur(); // necessary or price will not update
	await sleep(10);
	amountInput.value = contracts;
    amountInput[Object.keys(amountInput).filter((k) => k.startsWith('__reactProps'))[0]].onChange({'target' : { 'value' : contracts}});
	await sleep(10);

	if(isLongPosition) {
		simulateMouseClick([...document.querySelectorAll("div[class^='ordercard_order'] button")].filter(a => a.textContent.includes("Buy"))[0])
	} else {
		simulateMouseClick([...document.querySelectorAll("div[class^='ordercard_order'] button")].filter(a => a.textContent.includes("Sell"))[0])
	}

	let createdOrder = await waitUntilNotNull(() => {
		return getMostRecentOrder(mostRecentOrder?.id, x => {
			return x.direction == (isLongPosition ? 'Buy' : 'Sell')
				&& x.positionSize == contracts
				&& x.orderType == 'Stop Market' && x.stopPrice == price;
		});
	});

	return createdOrder;
}

async function cancelOrder(orderId) {
	var orderTab = document.querySelector("div[id^='ordersTab-']");

	var orderRecords = orderTab.querySelectorAll(".MuiDataGrid-virtualScroller div[role='row']");
	for (let i = 0; i < orderRecords.length; i++) {
		var mapped = mapOrderFromOrderList(orderRecords[i]);

		if(mapped.id != orderId) {
			continue;
		}
		var cancelButton = orderRecords[i].querySelector("div.MuiDataGrid-cell[data-field='cancel'] button");
		if(cancelButton != null) {
			simulateMouseClick(cancelButton);
		}
	}
	
	return null;
}

let instrument = 'MNQ';
let pnlPerPoint = 2;

function mapPositionFromPositionList(element) {
	var position = {};
	position.symbol = element.querySelector("div.MuiDataGrid-cell[data-field='symbolName']").innerText;
	position.positionSize = element.querySelector("div.MuiDataGrid-cell[data-field='positionSize']").innerText;
	position.entryPrice = element.querySelector("div.MuiDataGrid-cell[data-field='averagePrice']").innerText;
	position.risk = element.querySelector("div.MuiDataGrid-cell[data-field='risk']").innerText;
	position.toMake = element.querySelector("div.MuiDataGrid-cell[data-field='toMake']").innerText;

	// sanitize afterwards cuz lazy
	position.positionSize = position.positionSize ? parseInt(position.positionSize) : null;
	position.entryPrice = position.entryPrice ? parseFloat(position.entryPrice.replace('$','').replace(',', '')) : null;
	position.risk = position.risk ? parseFloat(position.risk.replace('$','').replace(',', '')) : null;
	position.toMake = position.toMake ? parseFloat(position.toMake.replace('$','').replace(',', '')) : null;

	return position;
}

async function setStopLoss(price) {
	// TODO: handle 0
	// TODO: handle stop in profit

	var positionTab = document.querySelector("div[id^='positionTab']");

	var positionRecords = positionTab.querySelectorAll(".MuiDataGrid-virtualScroller div[role='row']");

	for (let i = 0; i < positionRecords.length; i++) {
		var mapped = mapPositionFromPositionList(positionRecords[i]);
		if(mapped.symbol != '/' + instrument) continue;

		var riskToSet = -1;
		if(mapped.positionSize > 0) {
			// long
			riskToSet = Math.max(0, (mapped.entryPrice - price) * mapped.positionSize * pnlPerPoint);
		} else {
			// short
			riskToSet = Math.max(0, (price - mapped.entryPrice) * -1 * mapped.positionSize * pnlPerPoint);
		}
		
		// if difference is less than 1 tick, don't change.
		if(Math.abs(mapped.risk - riskToSet.toFixed(2)) >= 0.5) {
			var riskColumn = positionRecords[i].querySelector("div.MuiDataGrid-cell[data-field='risk']")
			simulateMouseDoubleClick(riskColumn);
			var inputField = await waitForElm("div[id^='positionTab'] div.MuiDataGrid-cell[data-field='risk'] input");

			inputField.value = riskToSet.toFixed(2);
			inputField[Object.keys(inputField).filter((k) => k.startsWith('__reactProps'))[0]].onChange({'target' : { 'value' : riskToSet.toFixed(2) }});
			await sleep(10);
			inputField[Object.keys(inputField).filter((k) => k.startsWith('__reactProps'))[0]].onBlur();
			await sleep(10);
			simulateMouseClick(positionTab);
		}
	}
}

async function setTakeProfit(price) {
	var positionTab = document.querySelector("div[id^='positionTab']");

	var positionRecords = positionTab.querySelectorAll(".MuiDataGrid-virtualScroller div[role='row']");

	for (let i = 0; i < positionRecords.length; i++) {
		var mapped = mapPositionFromPositionList(positionRecords[i]);
		if(mapped.symbol != '/' + instrument) continue;

		var toMakeToSet = -1;
		if(mapped.positionSize > 0) {
			// long
			toMakeToSet = Math.max(0, (price - mapped.entryPrice) * mapped.positionSize * pnlPerPoint);
		} else {
			// short
			toMakeToSet = Math.max(0, (mapped.entryPrice - price) * -1 * mapped.positionSize * pnlPerPoint);
		}

		//if(toMakeToSet.toFixed(2) != mapped.toMake) {
		// if difference is less than 1 tick, don't change.
		if(Math.abs(mapped.toMake - toMakeToSet.toFixed(2)) >= 0.5) {
			var toMakeColumn = positionRecords[i].querySelector("div.MuiDataGrid-cell[data-field='toMake']")
			simulateMouseDoubleClick(toMakeColumn);
			var inputField = await waitForElm("div[id^='positionTab'] div.MuiDataGrid-cell[data-field='toMake'] input");
			
			inputField.value = toMakeToSet.toFixed(2);
			inputField[Object.keys(inputField).filter((k) => k.startsWith('__reactProps'))[0]].onChange({'target' : { 'value' : toMakeToSet.toFixed(2) }});
			await sleep(10);
			inputField[Object.keys(inputField).filter((k) => k.startsWith('__reactProps'))[0]].onBlur();
			await sleep(10);
			simulateMouseClick(positionTab);
		}
	}
}

async function flatten() {
	var button = 
		[...document.querySelectorAll("div[class^='ordercard_order'] button")]
			.filter(a => a.textContent.includes("Flatten All"))[0];
			
	simulateMouseClick(button);
}


var enabled = false;
async function createConnectionButton() {
	// Add button
	const node = document.createElement("div");
	node.style['position'] = 'absolute';
	node.style['top'] = '400px';
	node.style['left'] = '0';
	node.style['display'] = 'block'
	node.style['width'] = '60px';
	node.style['height'] = '25px';
	node.style['z-index'] = 99999;
	node.style['background-color'] = 'red';
	node.id = 'hubconnectbutton';
	node.innerHTML = '<svg class="MuiSvgIcon-root MuiSvgIcon-fontSizeMedium css-o9gjpi" focusable="false" aria-hidden="true" viewBox="0 0 24 24"><path d="M 8 13 L 12 9 l 1 -3 L 15 8 l 0.94 2.06 L 16 13 l -2.06 0.94 L 12 14 l -2 0 z M 4 14 l 2 -1 L 7 11 l -1 -2 L 4 8 l -2 1 L 1 11 l 1 2 L 4 14 z m 4.5 -5 l 1.5 -1 L 12 5.5 L 11 3 L 8.5 2 L 6 3 L 5 5.5 l 1 1.5 L 8.5 9 z z"></path></svg>';

	node.onclick = function() {
		enabled = !enabled;
		
		if(enabled) connect();
		else connection.stop();
	}

	//var menuContainer = document.querySelector(".MuiBox-root > div[data-active='true']").parentNode
	//menuContainer.appendChild(node);
	document.body.appendChild(node);
}

const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5000/topstepxhub")
      .configureLogging(signalR.LogLevel.Information)
      .build();

connection.onclose(async () => {
	document.querySelector('#hubconnectbutton').style['background-color'] = 'orange';
	if(enabled) {
		await connect();
	}
});

async function connect() {
	if(enabled) {
		
		var optionToSelect = [...document.querySelectorAll("div[data-intercom-target='layout-selector'] select option")]
			.filter(a => a.textContent.includes("dataview") && !a.textContent.includes("draft"))[0].attributes['value'].value;
			
		if(document.querySelector("div[data-intercom-target='layout-selector'] select").value != optionToSelect) {
			setReactSelectValue(document.querySelector("div[data-intercom-target='layout-selector'] select"), optionToSelect)
		}

		document.querySelector('#hubconnectbutton').style['background-color'] = 'orange';
		
		try {
			await connection.start();
			await announceClient();
			document.querySelector('#hubconnectbutton').style['background-color'] = 'green';
		} catch (err) {
			document.querySelector('#hubconnectbutton').style['background-color'] = 'orange';
			setTimeout(connect, 5000);
		}
	}
}

async function announceClient() {
	var accountName = [...document.querySelectorAll("div[data-intercom-target='navbar'] div[role='combobox'] li div span")]
		.filter(a => a.textContent.includes("EXPRESS") || a.textContent.includes("S1") || a.textContent.includes('PRACTICE')).slice(-1)[0]
		.innerText.replace('(', '').replace(')', '');
	
	var instrument = document.querySelector("div[class^='ordercard_order'] > div:nth-child(1) input[role='combobox']").value;
	await connection.invoke("AnnounceConnected", accountName, instrument);
}


connection.on("CreateLimitOrder", async (isLong, price, quantity) =>
{
	var createdOrder = await createLimitOrder(isLong, price, quantity);
	if(createdOrder != null) {
		return { Success: true, OrderId: createdOrder.id };
	} else {
		return { Success: false };
	}
});

connection.on("CreateMarketOrder", async (isLong, quantity) => 
{
	var createdOrder = await createMarketOrder(isLong, quantity);
	if(createdOrder != null) {
		return { Success: true, OrderId: createdOrder.id };
	} else {
		return { Success: false };
	}
});

connection.on("CreateStopOrder", async (isLong, price, quantity) => {
	var createdOrder = await createStopOrder(isLong, price, quantity);
	if(createdOrder != null) {
		return { Success: true, OrderId: createdOrder.id };
	} else {
		return { Success: false };
	}
});

connection.on("SetTakeProfit", async (price) => {
	await setTakeProfit(price);
	return { Success: true };
});

connection.on("SetStopLoss", async (price) => {
	await setStopLoss(price);
	return { Success: true }; 
});

connection.on("CancelOrder", async (orderId) => {
	var result = await cancelOrder(orderId);
	
	return { Success: true };
});

connection.on("Flatten", async () => {
	var result = await flatten();
	
	return { Success: true };
});

window.addEventListener('load', async function() {
	await waitForElm("div[data-intercom-target='navbar'] div[role='combobox']");
	
    createConnectionButton();
}, false);


//var createdOrder = await createLimitOrder(1, 20310, 2);
//console.log("Created new order: " + JSON.stringify(createdOrder));
//
//var createdOrder = await createLimitOrder(1, 20510, 2);
//console.log("Created new order: " + JSON.stringify(createdOrder));


})();
	