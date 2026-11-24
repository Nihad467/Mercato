import { useEffect, useState } from 'react';
import {
  ArrowLeft, ArrowRight, BadgePercent, Boxes, ChartNoAxesCombined,
  ChevronDown, CircleHelp, ClipboardList, LayoutDashboard, Package,
  Pencil, Plus, RefreshCw, Search, Tag, Trash2, TriangleAlert, Users,
} from 'lucide-react';
import { Link, Navigate } from 'react-router-dom';
import { request } from './api.js';
import { useStore } from './store.jsx';

const money = (value) => new Intl.NumberFormat('az-AZ', {
  style: 'currency', currency: 'AZN', maximumFractionDigits: 2,
}).format(Number(value || 0));

const statusOptions = [
  ['Pending', 1], ['PaymentPending', 2], ['Paid', 3],
  ['PaymentFailed', 4], ['Cancelled', 5], ['Shipped', 6], ['Completed', 7],
];

function AdminPage() {
  const { token, email, isAdmin, signOut, notify } = useStore();
  const [section, setSection] = useState('overview');

  if (!token) return <Navigate to="/login" state={{ from: '/admin' }} replace />;
  if (!isAdmin) return <main className="page-width admin-denied"><span><CircleHelp size={26} /></span><span className="eyebrow">ADMIN GİRİŞİ</span><h1>Bu səhifə admin hesabı üçündür.</h1><p>Hazırkı hesabın idarə panelinə giriş hüququna malik deyil.</p><Link to="/" className="button button-dark">Mağazaya qayıt <ArrowLeft size={15} /></Link></main>;

  const sections = [
    { id: 'overview', label: 'İcmal', icon: LayoutDashboard },
    { id: 'products', label: 'Məhsullar', icon: Package },
    { id: 'categories', label: 'Kateqoriyalar', icon: Boxes },
    { id: 'orders', label: 'Sifarişlər', icon: ClipboardList },
    { id: 'coupons', label: 'Kuponlar', icon: BadgePercent },
  ];

  return <main className="admin-page page-width">
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <div className="admin-brand"><span className="admin-brand-mark">M</span><span><b>Mercato</b><small>İDARƏ PANELİ</small></span></div>
        <div className="admin-nav-caption">İDARƏETMƏ</div>
        <nav className="admin-nav">{sections.map(({ id, label, icon: Icon }) => <button key={id} className={section === id ? 'active' : ''} onClick={() => setSection(id)}><Icon size={17} />{label}{section === id && <span className="admin-nav-marker" />}</button>)}</nav>
        <div className="admin-sidebar-bottom"><Link to="/" className="admin-store-link"><ArrowLeft size={15} /> Mağazaya qayıt</Link><div className="admin-user"><span>{email.slice(0, 1).toUpperCase() || 'A'}</span><div><b>{email || 'Admin hesabı'}</b><small>Administrator</small></div><button title="Hesabdan çıx" onClick={signOut}><ArrowRight size={15} /></button></div></div>
      </aside>
      <section className="admin-main">
        <div className="admin-topbar"><div><span className="eyebrow">MERCATO / İDARƏETMƏ</span><h1>{sections.find((item) => item.id === section)?.label}</h1></div><div className="admin-topbar-right"><span className="admin-live"><i /> API-yə qoşulub</span><Link to="/" className="admin-view-store">Mağazanı gör <ArrowRight size={14} /></Link></div></div>
        {section === 'overview' && <Dashboard notify={notify} />}
        {section === 'products' && <ProductsAdmin notify={notify} />}
        {section === 'categories' && <CategoriesAdmin notify={notify} />}
        {section === 'orders' && <OrdersAdmin notify={notify} />}
        {section === 'coupons' && <CouponsAdmin notify={notify} />}
      </section>
    </div>
  </main>;
}

function Dashboard({ notify }) {
  const [data, setData] = useState({ summary: null, revenue: null, topProducts: [], lowStock: [], recentOrders: [], categoryStats: [] });
  const [loading, setLoading] = useState(true);
  const [stamp, setStamp] = useState(Date.now());

  useEffect(() => {
    setLoading(true);
    const urls = [
      '/admin/dashboard/summary', '/admin/dashboard/revenue-summary',
      '/admin/dashboard/top-products?take=5', '/admin/dashboard/low-stock?threshold=5',
      '/admin/dashboard/recent-orders?take=6', '/admin/dashboard/category-stats',
    ];
    Promise.all(urls.map((url) => request(url))).then(([summary, revenue, topProducts, lowStock, recentOrders, categoryStats]) => {
      setData({ summary, revenue, topProducts, lowStock, recentOrders, categoryStats });
    }).catch((error) => notify(error.message, 'error')).finally(() => setLoading(false));
  }, [notify, stamp]);

  const summary = data.summary || {};
  const revenue = data.revenue || {};
  const maxRevenue = Math.max(1, Number(revenue.totalRevenue || 0), Number(revenue.monthlyRevenue || 0));
  const stats = [
    { label: 'Ümumi gəlir', value: money(summary.totalRevenue), icon: ChartNoAxesCombined, tone: 'green' },
    { label: 'Sifarişlər', value: summary.totalOrders ?? '—', icon: ClipboardList, tone: 'blue' },
    { label: 'Məhsullar', value: summary.totalProducts ?? '—', icon: Package, tone: 'peach' },
    { label: 'İstifadəçilər', value: summary.totalUsers ?? '—', icon: Users, tone: 'lilac' },
  ];

  if (loading && !data.summary) return <AdminLoading />;
  return <div className="admin-dashboard">
    <div className="admin-welcome"><div><span className="eyebrow">İCMAL</span><h2>İşlər necə gedir?</h2><p>Mağazanın bugünkü göstəricilərinə ümumi baxış.</p></div><button className="admin-refresh" onClick={() => setStamp(Date.now())}><RefreshCw size={14} /> Yenilə</button></div>
    <div className="admin-stat-grid">{stats.map(({ label, value, icon: Icon, tone }) => <article className="admin-stat-card" key={label}><span className={`admin-stat-icon ${tone}`}><Icon size={18} /></span><span className="admin-stat-label">{label}</span><b>{loading ? '…' : value}</b></article>)}</div>
    <div className="admin-stat-grid admin-stat-grid-small"><article className="admin-stat-card stat-mini"><span className="admin-stat-icon gold"><TriangleAlert size={17} /></span><span className="admin-stat-label">Azalan stok</span><b>{summary.lowStockCount ?? data.lowStock.length}</b></article><article className="admin-stat-card stat-mini"><span className="admin-stat-icon rose"><ClipboardList size={17} /></span><span className="admin-stat-label">Gözləyən sifarişlər</span><b>{summary.pendingOrders ?? '—'}</b></article><article className="admin-stat-card stat-mini admin-stat-revenue"><span className="admin-stat-label">Bu ay</span><b>{money(revenue.monthlyRevenue)}</b><small>Bu həftə: {money(revenue.weeklyRevenue)}</small></article></div>
    <div className="admin-dashboard-columns"><section className="admin-panel"><div className="admin-panel-heading"><div><span className="eyebrow">SATIŞ GÖSTƏRİCİLƏRİ</span><h3>Gəlir icmalı</h3></div><span className="admin-panel-icon"><ChartNoAxesCombined size={16} /></span></div><div className="revenue-bars">{[['Bu gün', revenue.todayRevenue], ['Bu həftə', revenue.weeklyRevenue], ['Bu ay', revenue.monthlyRevenue], ['Cəmi', revenue.totalRevenue]].map(([label, amount]) => <div className="revenue-bar-row" key={label}><span>{label}</span><div><i style={{ width: `${Math.max(4, (Number(amount || 0) / maxRevenue) * 100)}%` }} /></div><b>{money(amount)}</b></div>)}</div><div className="top-products"><div className="admin-subheading"><b>Ən çox satılanlar</b><span>Top 5</span></div>{data.topProducts.length ? data.topProducts.map((product, index) => <div className="top-product-row" key={product.productId}><span className="rank">{String(index + 1).padStart(2, '0')}</span><span className="top-product-name">{product.productName}</span><span className="top-product-sold">{product.totalSold} satılıb</span><b>{money(product.totalRevenue)}</b></div>) : <AdminEmpty label="Satış məlumatı yoxdur." />}</div></section>
      <section className="admin-panel"><div className="admin-panel-heading"><div><span className="eyebrow">ANBAR</span><h3>Azalan stok</h3></div><span className="admin-panel-icon warning"><TriangleAlert size={16} /></span></div>{data.lowStock.length ? <div className="low-stock-list">{data.lowStock.slice(0, 6).map((item) => <div className="low-stock-row" key={item.productId}><span className="low-stock-dot" /><div><b>{item.productName}</b><small>{money(item.price)}</small></div><span className={Number(item.stock) <= 2 ? 'stock-critical' : 'stock-low'}>{item.stock} qalıb</span></div>)}</div> : <AdminEmpty label="Azalan stokda məhsul yoxdur." />}<div className="admin-subheading category-subheading"><b>Kateqoriyalar</b><span>{data.categoryStats.length} kateqoriya</span></div>{data.categoryStats.slice(0, 4).map((item) => <div className="category-stat-row" key={item.categoryId}><span>{item.categoryName}</span><b>{money(item.totalRevenue)}</b></div>)}</section></div>
    <section className="admin-panel recent-orders-panel"><div className="admin-panel-heading"><div><span className="eyebrow">SON HƏRƏKƏTLƏR</span><h3>Son sifarişlər</h3></div><span className="admin-panel-icon"><ClipboardList size={16} /></span></div><OrdersTable orders={data.recentOrders} compact /></section>
  </div>;
}

function AdminLoading() { return <div className="admin-loading"><span className="spinner" /> Məlumat yüklənir...</div>; }
function AdminEmpty({ label }) { return <div className="admin-empty"><Package size={17} />{label}</div>; }

function AdminSectionHeading({ eyebrow, title, action, onAction }) {
  return <div className="admin-section-heading"><div><span className="eyebrow">{eyebrow}</span><h2>{title}</h2></div>{action && <button className="button button-dark admin-add-button" onClick={onAction}><Plus size={15} />{action}</button>}</div>;
}

function ProductsAdmin({ notify }) {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editor, setEditor] = useState(null);
  const [search, setSearch] = useState('');
  const [stamp, setStamp] = useState(Date.now());
  const load = () => Promise.all([
    request('/Products?pageNumber=1&pageSize=50&sortBy=id&descending=true'),
    request('/Categories'),
  ]).then(([result, categoryData]) => {
    setProducts(result?.items || []);
    setCategories(Array.isArray(categoryData) ? categoryData : []);
  }).catch((error) => notify(error.message, 'error')).finally(() => setLoading(false));
  useEffect(() => { setLoading(true); load(); }, [notify, stamp]);

  const remove = async (product) => {
    if (!window.confirm(`“${product.name}” məhsulu silinsin?`)) return;
    try { await request(`/Products/${product.id}`, { method: 'DELETE' }); notify('Məhsul silindi.'); setStamp(Date.now()); }
    catch (error) { notify(error.message, 'error'); }
  };
  const filtered = products.filter((product) => `${product.name} ${product.categoryName}`.toLowerCase().includes(search.toLowerCase()));

  return <div className="admin-section"><AdminSectionHeading eyebrow="KATALOQ" title="Məhsullar" action="Məhsul əlavə et" onAction={() => setEditor({})} /><div className="admin-toolbar"><label className="admin-search"><Search size={15} /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Məhsul axtar..." /></label><span>{products.length} məhsul</span></div><div className="admin-table-wrap">{loading ? <AdminLoading /> : <table className="admin-table"><thead><tr><th>MƏHSUL</th><th>KATEQORİYA</th><th>QİYMƏT</th><th>STOK</th><th>ƏMƏLİYYAT</th></tr></thead><tbody>{filtered.map((product) => <tr key={product.id}><td><span className="table-product-icon"><Package size={16} /></span><b>{product.name}</b><small>MC-{String(product.id).padStart(4, '0')}</small></td><td>{product.categoryName}</td><td><b>{money(product.price)}</b></td><td><span className={`admin-stock-pill${product.stock <= 5 ? ' warning' : ''}`}>{product.stock} ədəd</span></td><td><div className="table-actions"><button onClick={() => setEditor(product)} title="Dəyiş"><Pencil size={15} /></button><button className="delete-action" onClick={() => remove(product)} title="Sil"><Trash2 size={15} /></button></div></td></tr>)}</tbody></table>}{!loading && filtered.length === 0 && <AdminEmpty label="Məhsul tapılmadı." />}</div>{editor && <ProductEditor product={editor.id ? editor : null} categories={categories} notify={notify} onClose={() => setEditor(null)} onSaved={() => { setEditor(null); setStamp(Date.now()); }} />}</div>;
}

function ProductEditor({ product, categories, notify, onClose, onSaved }) {
  const [form, setForm] = useState({ name: product?.name || '', description: '', price: product?.price || '', stock: product?.stock || '', categoryId: product?.categoryId || categories[0]?.id || '' });
  const [busy, setBusy] = useState(false);
  useEffect(() => {
    if (!product?.id) return;
    request(`/Products/${product.id}`).then((details) => setForm((current) => ({ ...current, description: details.description || '' }))).catch(() => {});
  }, [product?.id]);
  const change = (event) => setForm({ ...form, [event.target.name]: event.target.value });
  const submit = async (event) => {
    event.preventDefault();
    setBusy(true);
    const body = new FormData();
    body.append('Name', form.name.trim());
    body.append('Description', form.description.trim());
    body.append('Price', String(form.price));
    body.append('Stock', String(form.stock));
    body.append('CategoryId', String(form.categoryId));
    const images = event.currentTarget.elements.namedItem('Images')?.files || [];
    for (const file of images) body.append('Images', file);
    try {
      await request(product ? `/Products/${product.id}` : '/Products', { method: product ? 'PUT' : 'POST', body });
      notify(product ? 'Məhsul yeniləndi.' : 'Məhsul əlavə edildi.');
      onSaved();
    } catch (error) { notify(error.message, 'error'); }
    finally { setBusy(false); }
  };
  return <div className="admin-modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose(); }}><form className="admin-modal" onSubmit={submit}><div className="admin-modal-header"><div><span className="eyebrow">MƏHSUL KARTI</span><h3>{product ? 'Məhsulu yenilə' : 'Yeni məhsul'}</h3></div><button type="button" onClick={onClose} aria-label="Bağla">×</button></div><label>Məhsul adı<input name="name" value={form.name} onChange={change} required /></label><label>Təsvir<textarea name="description" value={form.description} onChange={change} rows="3" required /></label><div className="admin-form-grid"><label>Qiymət ($)<input name="price" type="number" min="0" step="0.01" value={form.price} onChange={change} required /></label><label>Stok sayı<input name="stock" type="number" min="0" value={form.stock} onChange={change} required /></label></div><label>Kateqoriya<select name="categoryId" value={form.categoryId} onChange={change} required>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label><label className="file-label">Şəkillər<input name="Images" type="file" accept="image/*" multiple /><small>İstəyə bağlı · JPG, PNG və ya WebP</small></label><div className="admin-modal-actions"><button type="button" className="button button-outline" onClick={onClose}>Ləğv et</button><button className="button button-dark" disabled={busy}>{busy ? 'Saxlanır...' : product ? 'Dəyişiklikləri saxla' : 'Məhsul yarat'}</button></div></form></div>;
}

function CategoriesAdmin({ notify }) {
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [name, setName] = useState('');
  const [editing, setEditing] = useState(null);
  const [busy, setBusy] = useState(false);
  const [stamp, setStamp] = useState(Date.now());
  useEffect(() => { request('/Categories').then((data) => setCategories(Array.isArray(data) ? data : [])).catch((error) => notify(error.message, 'error')).finally(() => setLoading(false)); }, [notify, stamp]);
  const save = async (event) => {
    event.preventDefault(); setBusy(true);
    try {
      await request(editing ? `/Categories/${editing.id}` : '/Categories', { method: editing ? 'PUT' : 'POST', body: editing ? { id: editing.id, name: name.trim() } : { name: name.trim() } });
      notify(editing ? 'Kateqoriya yeniləndi.' : 'Kateqoriya əlavə edildi.'); setName(''); setEditing(null); setStamp(Date.now());
    } catch (error) { notify(error.message, 'error'); }
    finally { setBusy(false); }
  };
  const remove = async (category) => {
    if (!window.confirm(`“${category.name}” kateqoriyası silinsin?`)) return;
    try { await request(`/Categories/${category.id}`, { method: 'DELETE' }); notify('Kateqoriya silindi.'); setStamp(Date.now()); }
    catch (error) { notify(error.message, 'error'); }
  };
  return <div className="admin-section"><AdminSectionHeading eyebrow="KATALOQ" title="Kateqoriyalar" /><div className="admin-two-column"><form className="admin-panel admin-inline-form" onSubmit={save}><span className="eyebrow">{editing ? 'KATEQORİYANI DƏYİŞ' : 'YENİ KATEQORİYA'}</span><h3>{editing ? 'Adı yenilə' : 'Kateqoriya yarat'}</h3><label>Kateqoriya adı<input value={name} onChange={(event) => setName(event.target.value)} placeholder="Məsələn, Ev üçün" required /></label><div className="inline-form-actions"><button className="button button-dark" disabled={busy}>{busy ? 'Saxlanır...' : editing ? 'Yenilə' : 'Əlavə et'} <ArrowRight size={14} /></button>{editing && <button type="button" className="button button-outline" onClick={() => { setEditing(null); setName(''); }}>Ləğv et</button>}</div></form><section className="admin-panel category-admin-list"><div className="admin-panel-heading"><div><span className="eyebrow">MÖVCUD KATEQORİYALAR</span><h3>{categories.length} kateqoriya</h3></div><Boxes size={18} /></div>{loading ? <AdminLoading /> : categories.length ? categories.map((category) => <div className="category-admin-row" key={category.id}><span className="category-admin-icon"><Tag size={15} /></span><b>{category.name}</b><small>#{category.id}</small><button onClick={() => { setEditing(category); setName(category.name); }} title="Dəyiş"><Pencil size={14} /></button><button className="delete-action" onClick={() => remove(category)} title="Sil"><Trash2 size={14} /></button></div>) : <AdminEmpty label="Kateqoriya yoxdur." />}</section></div></div>;
}

function OrdersAdmin({ notify }) {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [stamp, setStamp] = useState(Date.now());
  useEffect(() => { request('/admin/orders').then((data) => setOrders(Array.isArray(data) ? data : [])).catch((error) => notify(error.message, 'error')).finally(() => setLoading(false)); }, [notify, stamp]);
  const changeStatus = async (order, value) => {
    try { await request('/admin/orders/status', { method: 'PUT', body: { orderId: order.id, status: Number(value) } }); notify(`№${order.id} sifarişinin statusu yeniləndi.`); setStamp(Date.now()); }
    catch (error) { notify(error.message, 'error'); }
  };
  return <div className="admin-section"><AdminSectionHeading eyebrow="SİFARİŞLƏR" title="Bütün sifarişlər" /><div className="admin-toolbar"><span>Ümumi {orders.length} sifariş</span><button className="admin-refresh" onClick={() => setStamp(Date.now())}><RefreshCw size={14} /> Yenilə</button></div>{loading ? <AdminLoading /> : <section className="admin-panel"><OrdersTable orders={orders} onStatusChange={changeStatus} /></section>}</div>;
}

function OrdersTable({ orders, onStatusChange, compact = false }) {
  if (!orders.length) return <AdminEmpty label="Sifariş yoxdur." />;
  return <div className="admin-table-wrap"><table className="admin-table"><thead><tr><th>SİFARİŞ</th><th>TARİX</th><th>MƏHSULLAR</th><th>MƏBLƏĞ</th><th>STATUS</th>{onStatusChange && <th>YENİLƏ</th>}</tr></thead><tbody>{orders.map((order) => <tr key={order.id}><td><b>#{String(order.id).padStart(5, '0')}</b></td><td>{new Date(order.createdAtUtc).toLocaleDateString('az-AZ')}</td><td><span className="order-items-summary">{order.items?.map((item) => `${item.quantity} × ${item.productName}`).join(', ') || '—'}</span></td><td><b>{money(order.totalPrice)}</b></td><td><span className={`order-status status-${String(order.status).toLowerCase()}`}>{order.status}</span></td>{onStatusChange && <td><label className="status-select"><select value={statusOptions.find(([label]) => label.toLowerCase() === String(order.status).toLowerCase())?.[1] || 1} onChange={(event) => onStatusChange(order, event.target.value)}>{statusOptions.map(([label, value]) => <option key={value} value={value}>{label}</option>)}</select><ChevronDown size={12} /></label></td>}</tr>)}</tbody></table></div>;
}

function CouponsAdmin({ notify }) {
  const [coupons, setCoupons] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(null);
  const [stamp, setStamp] = useState(Date.now());
  useEffect(() => { request('/Coupons').then((data) => setCoupons(Array.isArray(data) ? data : [])).catch((error) => notify(error.message, 'error')).finally(() => setLoading(false)); }, [notify, stamp]);
  const remove = async (coupon) => {
    if (!window.confirm(`${coupon.code} kuponu silinsin?`)) return;
    try { await request(`/Coupons/${coupon.id}`, { method: 'DELETE' }); notify('Kupon silindi.'); setStamp(Date.now()); }
    catch (error) { notify(error.message, 'error'); }
  };
  return <div className="admin-section"><AdminSectionHeading eyebrow="ENDİRİMLƏR" title="Kuponlar" action="Kupon yarat" onAction={() => setEditing({})} /><div className="admin-toolbar"><span>{coupons.length} kupon</span><button className="admin-refresh" onClick={() => setStamp(Date.now())}><RefreshCw size={14} /> Yenilə</button></div><div className="admin-table-wrap">{loading ? <AdminLoading /> : <table className="admin-table"><thead><tr><th>KOD</th><th>ENDİRİM</th><th>BİTMƏ TARİXİ</th><th>İSTİFADƏ</th><th>STATUS</th><th>ƏMƏLİYYAT</th></tr></thead><tbody>{coupons.map((coupon) => <tr key={coupon.id}><td><span className="coupon-code"><Tag size={14} />{coupon.code}</span></td><td>{Number(coupon.discountType) === 1 ? `${coupon.discountValue}%` : money(coupon.discountValue)}</td><td>{new Date(coupon.expireDate).toLocaleDateString('az-AZ')}</td><td>{coupon.usedCount} / {coupon.usageLimit}</td><td><span className={`coupon-status ${coupon.isActive ? 'active' : 'inactive'}`}>{coupon.isActive ? 'Aktiv' : 'Deaktiv'}</span></td><td><div className="table-actions"><button onClick={() => setEditing(coupon)} title="Dəyiş"><Pencil size={15} /></button><button className="delete-action" onClick={() => remove(coupon)} title="Sil"><Trash2 size={15} /></button></div></td></tr>)}</tbody></table>}{!loading && coupons.length === 0 && <AdminEmpty label="Kupon yoxdur." />}</div>{editing && <CouponEditor coupon={editing.id ? editing : null} notify={notify} onClose={() => setEditing(null)} onSaved={() => { setEditing(null); setStamp(Date.now()); }} />}</div>;
}

function CouponEditor({ coupon, notify, onClose, onSaved }) {
  const [form, setForm] = useState({ code: coupon?.code || '', discountType: String(coupon?.discountType || 1), discountValue: coupon?.discountValue || '', expireDate: coupon?.expireDate ? String(coupon.expireDate).slice(0, 10) : '', usageLimit: coupon?.usageLimit || 50, isActive: coupon?.isActive ?? true });
  const [busy, setBusy] = useState(false);
  const change = (event) => setForm({ ...form, [event.target.name]: event.target.type === 'checkbox' ? event.target.checked : event.target.value });
  const submit = async (event) => {
    event.preventDefault(); setBusy(true);
    const body = { id: coupon?.id, code: form.code.trim().toUpperCase(), discountType: Number(form.discountType), discountValue: Number(form.discountValue), expireDate: new Date(`${form.expireDate}T23:59:59`).toISOString(), usageLimit: Number(form.usageLimit), isActive: Boolean(form.isActive) };
    try {
      if (coupon) await request(`/Coupons/${coupon.id}`, { method: 'PUT', body });
      else { delete body.id; await request('/Coupons', { method: 'POST', body }); }
      notify(coupon ? 'Kupon yeniləndi.' : 'Kupon yaradıldı.'); onSaved();
    } catch (error) { notify(error.message, 'error'); }
    finally { setBusy(false); }
  };
  return <div className="admin-modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose(); }}><form className="admin-modal" onSubmit={submit}><div className="admin-modal-header"><div><span className="eyebrow">ENDİRİM KODU</span><h3>{coupon ? 'Kuponu yenilə' : 'Yeni kupon'}</h3></div><button type="button" onClick={onClose} aria-label="Bağla">×</button></div><label>Kupon kodu<input name="code" value={form.code} onChange={change} placeholder="MERCATO10" required /></label><div className="admin-form-grid"><label>Endirim növü<select name="discountType" value={form.discountType} onChange={change}><option value="1">Faizlə</option><option value="2">Sabit məbləğ</option></select></label><label>Dəyər<input name="discountValue" type="number" min="0.01" step="0.01" value={form.discountValue} onChange={change} required /></label></div><div className="admin-form-grid"><label>Bitmə tarixi<input name="expireDate" type="date" value={form.expireDate} onChange={change} required /></label><label>İstifadə limiti<input name="usageLimit" type="number" min="1" value={form.usageLimit} onChange={change} required /></label></div><label className="admin-checkbox"><input type="checkbox" name="isActive" checked={form.isActive} onChange={change} /> Kupon aktivdir</label><div className="admin-modal-actions"><button type="button" className="button button-outline" onClick={onClose}>Ləğv et</button><button className="button button-dark" disabled={busy}>{busy ? 'Saxlanır...' : coupon ? 'Dəyişiklikləri saxla' : 'Kupon yarat'}</button></div></form></div>;
}

export default AdminPage;
