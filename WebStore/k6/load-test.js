import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';


export let orderRequestDuration = new Trend('order_req_duration');
export let productRequestDuration = new Trend('product_req_duration');

export let options = {
    stages: [
        { duration: '30s', target: 50 },  // Ramp up to 50 users in 30s
        { duration: '1m', target: 100 },  // Stay at 100 users for 1 minute
        { duration: '30s', target: 0 }    // Ramp down to 0 users in 30s
    ],
    thresholds: {
        //  Ensure 95% of requests complete within 500ms
        http_req_duration: ['p(95)<500'],

        // Ensure failure rate stays below 2%
        http_req_failed: ['rate<0.02'],
    }
};

export default function () {
    
    let productsRes = http.get('http://localhost:5205/api/v1/product');
    productRequestDuration.add(productsRes.timings.duration);
    check(productsRes, {
        'All Products - status 200': (r) => r.status === 200,
        'All Products - response < 500ms': (r) => r.timings.duration < 500,
    });

    
    let ordersRes = http.get('http://localhost:5205/api/v1/order');
    orderRequestDuration.add(ordersRes.timings.duration);
    check(ordersRes, {
        'All Orders - status 200': (r) => r.status === 200,
        'All Orders - response < 500ms': (r) => r.timings.duration < 500,
    });

    sleep(1);
}
