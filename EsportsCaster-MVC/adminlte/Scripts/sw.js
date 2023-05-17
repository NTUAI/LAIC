self.addEventListener('fetch', event => {
    event.respondWith(
        (async function () {
            // Fetch request
            const response = await fetch(event.request);
            // Return response
            return response;
        })()
    );
});
