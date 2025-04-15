/**
 * Метод для обновления страницы оплаты.
 */
function updatePayment() {
    console.log("update payment");
    const inputs = document.querySelectorAll('input[data-denomination]');
    let inserted = 0;
    const coinsInserted = {};

    inputs.forEach(input => {
        const denomination = parseInt(input.dataset.denomination);
        const quantity = parseInt(input.value) || 0;
        inserted += denomination * quantity;
        coinsInserted[denomination] = quantity;
    });

    const totalAmount = parseFloat('@Model.TotalAmount');
    const insertedAmount = document.getElementById('insertedAmount');
    insertedAmount.textContent = inserted;
    insertedAmount.style.color = inserted >= totalAmount ? 'green' : 'red';

    const payButton = document.getElementById('payButton');
    payButton.disabled = inserted < totalAmount;

    payButton.onclick = () => {
        fetch('/Home/ProcessPayment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(coinsInserted)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    const change = Object.entries(data.change)
                        .map(([denom, qty]) => `${qty} x ${denom} руб.`)
                        .join(', ');
                    document.getElementById('change').textContent = change;
                    document.getElementById('result').style.display = 'block';
                    payButton.style.display = 'none';
                } else {
                    alert(data.message);
                }
            });
    };
}