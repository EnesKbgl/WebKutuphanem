document.addEventListener('DOMContentLoaded', function () {

    // --- ÖNEMLİ: Verileri sunucudan çekiyoruz ---
    fetch('/Home/GetChartData')
        .then(response => response.json())
        .then(data => {
            console.log("Veriler alındı:", data); // Tarayıcı konsolunda (F12) bunu görmelisin
            initCharts(data);
        })
        .catch(error => console.error('Grafik verisi alınamadı:', error));

    function initCharts(data) {

        // Genel Ayarlar
        Chart.defaults.font.family = 'Inter, system-ui, sans-serif';
        Chart.defaults.color = '#64748B';

        const colors = {
            primary: '#4F46E5',
            success: '#10B981',
            warning: '#F59E0B',
            danger: '#EF4444',
            gray: '#6B7280'
        };

        // 1. PASTA GRAFİK (Veritabanından gelen sayılarla)
        const ctxStatus = document.getElementById('statusDistributionChart');
        if (ctxStatus) {
            new Chart(ctxStatus, {
                type: 'doughnut',
                data: {
                    labels: ['Tamamlanan', 'Okuyor', 'Okunacak'],
                    datasets: [{
                        data: [data.statusCounts.finished, data.statusCounts.reading, data.statusCounts.toRead],
                        backgroundColor: [colors.success, colors.warning, colors.gray],
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    cutout: '70%'
                }
            });
        }

        // 2. YATAY BAR GRAFİK (Veritabanından gelen yazarlar)
        const ctxAuthors = document.getElementById('topAuthorsChart');
        if (ctxAuthors) {
            new Chart(ctxAuthors, {
                type: 'bar',
                data: {
                    labels: data.topAuthors.map(a => a.author),
                    datasets: [{
                        label: 'Kitap Sayısı',
                        data: data.topAuthors.map(a => a.count),
                        backgroundColor: colors.primary,
                        borderRadius: 4
                    }]
                },
                options: {
                    indexAxis: 'y',
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: { x: { beginAtZero: true, ticks: { stepSize: 1 } } },
                    plugins: { legend: { display: false } }
                }
            });
        }

        // 3. ÇİZGİ GRAFİK (Aylık Veri - Eğer veri yoksa boş gelir)
        const ctxMonthly = document.getElementById('monthlyProgressChart');
        if (ctxMonthly) {
            // Eğer veritabanından aylık veri geldiyse onu kullan, yoksa boş göster
            const labels = data.monthlyData.length > 0 ? data.monthlyData.map(d => d.month) : ['Veri Yok'];
            const counts = data.monthlyData.length > 0 ? data.monthlyData.map(d => d.count) : [0];

            new Chart(ctxMonthly, {
                type: 'bar', // Çizgi yerine bar daha şık durabilir az veriyle
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Eklenen Kitap',
                        data: counts,
                        backgroundColor: colors.primary,
                        borderRadius: 4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
                }
            });
        }

        // 4. GÜNLÜK AKTİVİTE (Burası şimdilik statik kalabilir, veri modelimizde günlük okuma yok)
        const ctxActivity = document.getElementById('dailyActivityChart');
        if (ctxActivity) {
            new Chart(ctxActivity, {
                type: 'line',
                data: {
                    labels: ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'],
                    datasets: [{
                        label: 'Okuma Aktivitesi',
                        data: [0, 0, 0, 0, 0, 0, 0], // Şimdilik boş
                        borderColor: colors.gray,
                        borderDash: [5, 5],
                        fill: false
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false },
                        title: { display: true, text: 'Günlük okuma takibi henüz aktif değil' }
                    },
                    scales: { y: { beginAtZero: true } }
                }
            });
        }
    }
});