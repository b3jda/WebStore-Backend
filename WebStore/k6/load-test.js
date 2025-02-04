import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';

// Custom metrics
export let orderRequestDuration = new Trend('order_req_duration');
export let productRequestDuration = new Trend('product_req_duration');
export let failedRequests = new Rate('failed_requests');

// Load Test Configuration (Optimized for Stability)
export let options = {
    stages: [
        { duration: '20s', target: 5 },   // Slowly ramp up to 5 users
        { duration: '40s', target: 10 },  // Hold at 10 users (reduced load)
        { duration: '30s', target: 5 },   // Ramp down to 5 users
    ],
    thresholds: {
        http_req_duration: ['p(95)<2000'], // Allow up to 2s response time
        http_req_failed: ['rate<0.20'],    // Allow up to 20% failure rate
        failed_requests: ['rate<0.20'],    // Allow up to 20% failed requests
    },
};

// Helper function for retries and handling transient failures
function resilientRequest(url, maxRetries = 5) {
    let delay = 1;
    let res;

    for (let attempt = 0; attempt <= maxRetries; attempt++) {
        res = http.get(url, { timeout: '30s' });

        if (res.status === 200) {
            return res;
        }

        sleep(delay + Math.random() * 3);  // Add random wait time (0-3s)
        delay *= 2;  // Exponential backoff
    }

    failedRequests.add(1);
    return res;
}

// Main Test Function
export default function () {
    let productsRes = resilientRequest('http://localhost:5205/api/v1/product');
    productRequestDuration.add(productsRes.timings.duration);
    check(productsRes, {
        'All Products - status 200': (r) => r.status === 200,
        'All Products - response < 2000ms': (r) => r.timings.duration < 2000,
    });

    let ordersRes = resilientRequest('http://localhost:5205/api/v1/order');
    orderRequestDuration.add(ordersRes.timings.duration);
    check(ordersRes, {
        'All Orders - status 200': (r) => r.status === 200,
        'All Orders - response < 2000ms': (r) => r.timings.duration < 2000,
    });

    sleep(1);
}
