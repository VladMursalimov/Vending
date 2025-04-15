/**
 * Добавление обработчиков событий страницы (нажатия кнопки, изменение данных)
 */
document.addEventListener('DOMContentLoaded', () => {
    const brandFilter = document.getElementById('brandFilter');
    const priceSlider = document.getElementById('priceSlider');
    const toOrderBtn = document.getElementById('toOrder');

    brandFilter.addEventListener('change', () => {
        updateCatalog();
    });

    priceSlider.addEventListener('input', () => {
        document.getElementById('priceValue').textContent = `0 - ${priceSlider.value}`;
        updateCatalog();
    });

    toOrderBtn.addEventListener('click', () => {
        window.location.href = '/Home/Order';
    });

    function updateCatalog() {
        const brand = brandFilter.value;
        const maxPrice = priceSlider.value;
        window.location.href = `/Home/Index?brand=${brand}&maxPrice=${maxPrice}`;
    }
});

/**
 * Метод для добавления продукта в корзину.
 * @param {any} productId id продукта
 */
function addToCart(productId) {
    fetch('/Home/AddToCart', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const toOrderBtn = document.getElementById('toOrder');
                toOrderBtn.textContent = `Выбрано (${data.cartCount})`;
                toOrderBtn.disabled = false;
            }
        });
}


/**
 * Метод для обработки нажатия кнопки импорта.
 */
document.querySelector('form[action="/Home/UploadProducts"]').addEventListener('submit', async (event) => {
    event.preventDefault();
    console.log('Upload form submitted');

    const form = event.target;
    const formData = new FormData(form);
    const uploadMessage = document.getElementById('uploadMessage');
    const uploadError = document.getElementById('uploadError');

    uploadMessage.style.display = 'none';
    uploadError.style.display = 'none';

    try {
        const response = await fetch('/Home/UploadProducts', {
            method: 'POST',
            body: formData
        });

        console.log('Upload response:', response);
        const data = await response.json();
        console.log('Upload data:', data);

        if (data.success) {
            uploadMessage.textContent = data.message;
            uploadMessage.style.display = 'block';
            form.reset();
            setTimeout(() => location.reload(), 2000);
        } else {
            uploadError.textContent = data.message;
            uploadError.style.display = 'block';
        }
    } catch (error) {
        console.error('Error uploading file:', error);
        uploadError.textContent = 'Произошла ошибка при загрузке файла';
        uploadError.style.display = 'block';
    }
});