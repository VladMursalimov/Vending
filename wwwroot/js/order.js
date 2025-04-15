
/**
 * Метод для обновления кол-ва товара в заказе.
 * @param {any} input кол-во товара.
 * @param {any} productId id продукта.
 */
function updateQuantity(input, productId) {
    const quantity = parseInt(input.value) || 0;
    console.log(`updateQuantity called for productId: ${productId}, quantity: ${quantity}`);

    const itemElement = input.closest('.order-item');
    const pricePerUnit = parseFloat(itemElement.dataset.price);
    const itemPrice = itemElement.querySelector('.item-price');
    itemPrice.textContent = (quantity * pricePerUnit).toFixed(2);
    console.log(`Updated price for productId: ${productId} to ${(quantity * pricePerUnit).toFixed(2)}`);

    const items = Array.from(document.querySelectorAll('.order-item')).map(item => ({
        productId: parseInt(item.dataset.id),
        name: item.querySelector('span').textContent,
        quantity: parseInt(item.querySelector('input').value) || 0,
        price: parseFloat(item.dataset.price)
    }));

    fetch('/Home/UpdateOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(items)
    })
        .then(response => {
            console.log('UpdateOrder response:', response);
            if (!response.ok) {
                throw new Error('Update failed');
            }
            return response.json();
        })
        .then(data => {
            console.log('UpdateOrder data:', data);
            if (!data.success) {
                alert(data.message || 'Ошибка при обновлении заказа');
                input.value = input.defaultValue;
                itemPrice.textContent = (parseInt(input.defaultValue) * pricePerUnit).toFixed(2);
            } else {
                input.defaultValue = input.value; 
            }
        })
        .catch(error => {
            console.error('Error in updateQuantity:', error);
            alert('Произошла ошибка при обновлении заказа');
            input.value = input.defaultValue;
            itemPrice.textContent = (parseInt(input.defaultValue) * pricePerUnit).toFixed(2);
        });
}

/**
 * Метод для удаления товара из заказа.
 * @param {any} productId id продукта.
 */
function removeItem(productId) {
    console.log(`removeItem called for productId: ${productId}`);
    const items = Array.from(document.querySelectorAll('.order-item'))
        .filter(item => parseInt(item.dataset.id) !== productId)
        .map(item => ({
            productId: parseInt(item.dataset.id),
            name: item.querySelector('span').textContent,
            quantity: parseInt(item.querySelector('input').value) || 0,
            price: parseFloat(item.dataset.price)
        }));

    fetch('/Home/UpdateOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(items)
    })
        .then(response => {
            console.log('UpdateOrder response:', response);
            if (!response.ok) throw new Error('Remove failed');
            return response.json();
        })
        .then(data => {
            console.log('UpdateOrder data:', data);
            if (data.success) {
                document.querySelector(`.order-item[data-id="${productId}"]`).remove();
                if (!document.querySelectorAll('.order-item').length) {
                    window.location.href = '/';
                }
            } else {
                alert(data.message || 'Ошибка при удалении товара');
            }
        })
        .catch(error => {
            console.error('Error in removeItem:', error);
            alert('Произошла ошибка при удалении товара');
        });
}

/**
 * Метод для обновления заказа.
 */
function updateOrder() {
    const items = document.querySelectorAll('.order-item');
    const updatedItems = [];

    items.forEach(item => {
        updatedItems.push({
            ProductId: parseInt(item.dataset.id),
            Quantity: parseInt(item.querySelector('input').value)
        });
    });

    fetch('/Home/UpdateOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(updatedItems)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success && items.length === 0) {
                window.location.reload();
            }
        });
}
/**
 * Метод для обновления общей стоимости заказа.
 */
function updateTotals() {
    let total = 0;
    document.querySelectorAll('.order-item').forEach(item => {
        const quantity = parseInt(item.querySelector('input').value);
        const price = parseFloat(item.querySelector('.item-price').textContent) / quantity;
        const itemTotal = quantity * price;
        item.querySelector('.item-price').textContent = itemTotal;
        total += itemTotal;
    });
    document.getElementById('totalAmount').textContent = total;
}