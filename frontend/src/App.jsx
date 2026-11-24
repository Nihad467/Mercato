import { useEffect, useMemo, useState } from 'react';
import {
  ArrowDownRight, ArrowLeft, ArrowRight, ArrowUpRight, BadgeCheck, Check,
  ChevronDown, ChevronLeft, ChevronRight, CircleHelp, Clock3, CreditCard,
  Heart, Headphones, LampDesk, Leaf, LockKeyhole, LogIn, Menu, Minus,
  Package, Plus, Search, Send, ShieldCheck, Shirt, ShoppingBag, Sparkles,
  Star, Tag, Trash2, Truck, UserRound, Watch, X, Camera, Coffee, Laptop,
  Smartphone, Tablet, Monitor, Keyboard, Mouse, Router, Cable, HardDrive,
  Webcam, PlugZap, SlidersHorizontal,
} from 'lucide-react';
import {
  Link, Navigate, Route, Routes, useLocation, useNavigate, useParams,
  useSearchParams,
} from 'react-router-dom';
import { request } from './api.js';
import { useStore } from './store.jsx';
import AdminPage from './Admin.jsx';

const paymentProvider = String(import.meta.env.VITE_PAYMENT_PROVIDER || 'Stripe').toLowerCase() === 'mock' ? 'Mock' : 'Stripe';
const money = (value) => new Intl.NumberFormat('az-AZ', {
  style: 'currency', currency: 'AZN', maximumFractionDigits: 2,
}).format(Number(value || 0));

function Stars({ value = 0 }) {
  const count = Math.max(0, Math.min(5, Math.round(Number(value) || 0)));
  return <span className="rating-stars" aria-label={`${value} / 5`}>{'★'.repeat(count)}{'☆'.repeat(5 - count)}</span>;
}

const categoryArt = (product) => {
  const name = String(typeof product === 'string' ? product : product?.name || product?.productName || '').toLowerCase();
  const category = String(typeof product === 'string' ? product : product?.categoryName || '').toLowerCase();
  if (/tablet|ipad|galaxy tab|matepad|surface pro|planşet/.test(name) || /tablet|planşet/.test(category)) return Tablet;
  if (/laptop|macbook|legion|zenbook|victus|xps|katana|ideapad|notbuk|noutbuk/.test(name) || /laptop|notbuk|noutbuk/.test(category)) return Laptop;
  if (/monitor|ultragear|odyssey|ultrasharp|mobiuz/.test(name) || /monitor/.test(category)) return Monitor;
  if (/keyboard|blackwidow|keychron|apex pro|huntsman|mx keys|k70 rgb|klaviatura/.test(name) || /keyboard|klaviatura/.test(category)) return Keyboard;
  if (/mouse|mice|g502|deathadder|mx master|model o|rival 5|basilisk|superlight|siçan|mouse pad/.test(name) || /mouse|mice|siçan/.test(category)) return Mouse;
  if (/headset|headphone|arctis|cloud ii|wh-1000|quantum 800|qulaqlıq|qulaqcıq/.test(name) || /headset|qulaqlıq|qulaqcıq/.test(category)) return Headphones;
  if (/accessor|aksessuar/.test(category)) return /charger|power.?bank|adapter|cable|naqil|enerji|şüşə|tutacaq/.test(name) ? PlugZap : ShoppingBag;
  if (/iphone|smartphone|mobile phone|pixel|xiaomi|oneplus|galaxy s\d|phone|telefon|tecno|redmi|honor|oppo/.test(name) || /phone|telefon/.test(category)) return Smartphone;
  if (/router|archer ax/.test(name)) return Router;
  if (/usb.?c cable|cable|usb hub|hub/.test(name)) return Cable;
  if (/charger|power adapter|adapter/.test(name)) return PlugZap;
  if (/ssd|hard.?drive|portable storage/.test(name)) return HardDrive;
  if (/webcam|c920/.test(name)) return Webcam;
  if (/stream deck|control deck/.test(name)) return SlidersHorizontal;
  if (/wear|fashion|cloth|shirt|apparel|geyim/.test(name) || /wear|fashion|clothing/.test(category)) return Shirt;
  if (/home|lamp|furniture|interior/.test(name) || /home|furniture/.test(category)) return LampDesk;
  if (/watch|jewel|saat/.test(name) || /watch|jewel/.test(category)) return Watch;
  if (/camera|photo|foto/.test(name) || /camera/.test(category)) return Camera;
  if (/coffee|kitchen|food|mətbəx/.test(name) || /kitchen|food/.test(category)) return Coffee;
  if (/computer|tech|electronic/.test(name) || /computer|electronics/.test(category)) return Laptop;
  if (/accessor/.test(category)) return ShoppingBag;
  return Package;
};

const categoryTones = ['sage', 'peach', 'lilac', 'sky', 'butter'];
const productTone = (product) => categoryTones[Math.abs(Number(product?.id || product?.productId || 0)) % categoryTones.length];

function Artwork({ product, large = false }) {
  const Icon = categoryArt(product);
  const tone = productTone(product);
  const productId = product?.id ?? product?.productId;
  const [imageFailed, setImageFailed] = useState(false);

  useEffect(() => {
    setImageFailed(false);
  }, [productId]);

  return (
    <div className={`artwork tone-${tone}${large ? ' artwork-large' : ''}`}>
      <span className="artwork-glow" />
      <span className="artwork-disc" />
      <span className="artwork-shadow" />
      <div className="artwork-object"><Icon strokeWidth={1.15} /></div>
      <span className="artwork-mark">M / 01</span>
      {productId && !imageFailed && <img
        className="artwork-photo"
        src={`/api/Products/${productId}/image`}
        alt={product?.name || product?.productName || 'Məhsul şəkli'}
        loading={large ? 'eager' : 'lazy'}
        onError={() => setImageFailed(true)}
      />}
    </div>
  );
}

function Wordmark({ dark = false }) {
  return <Link to="/" className={`wordmark${dark ? ' wordmark-dark' : ''}`} aria-label="Mercato ana səhifə">
    <span className="wordmark-icon"><ShoppingBag size={18} strokeWidth={2.2} /></span>
    <span>mercato<span className="wordmark-dot">.</span></span>
  </Link>;
}

function Header() {
  const { token, cartCount, isAdmin } = useStore();
  const location = useLocation();
  const navigate = useNavigate();
  const [search, setSearch] = useState('');

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    setSearch(params.get('search') || '');
  }, [location.search]);

  const submitSearch = (event) => {
    event.preventDefault();
    const value = search.trim();
    navigate(value ? `/products?search=${encodeURIComponent(value)}` : '/products');
  };

  return (
    <>
      <div className="announcement"><span>Günün seçimi, sənin tərzin.</span><span className="announcement-right"><Sparkles size={13} /> Sənin üçün seçilmiş gündəlik favoritlər</span></div>
      <header className="site-header">
        <div className="header-main page-width">
          <button className="icon-button mobile-menu" aria-label="Menyu" onClick={() => navigate('/products')}><Menu size={20} /></button>
          <Wordmark />
          <form className="header-search" onSubmit={submitSearch}>
            <Search size={18} />
            <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Məhsul adı ilə axtar" aria-label="Məhsul axtar" />
            <button type="submit" aria-label="Axtar"><ArrowRight size={17} /></button>
          </form>
          <div className="header-actions">
            <Link to={token ? '/account' : '/login'} className="header-action account-action"><UserRound size={19} /><span>{token ? 'Hesabım' : 'Daxil ol'}</span></Link>
            <Link to="/wishlist" className="header-action wishlist-action"><Heart size={19} /><span>Seçilmişlər</span></Link>
            <Link to="/cart" className="header-action cart-action"><span className="cart-icon-wrap"><ShoppingBag size={20} />{cartCount > 0 && <b>{cartCount}</b>}</span><span>Səbət</span></Link>
          </div>
        </div>
        <nav className="main-nav page-width" aria-label="Əsas menyu">
          <Link to="/products" className="nav-categories"><Menu size={15} /> Bütün kateqoriyalar <ChevronDown size={14} /></Link>
          <div className="nav-links"><Link to="/products?sortBy=id&descending=true">Yenilər</Link><Link to="/products?sortBy=price&descending=false">Qiyməti münasib</Link><Link to="/products?sortBy=popular&descending=true">Trenddə</Link></div>
          <Link to={isAdmin ? '/admin' : '/products'} className="nav-promo">{isAdmin ? 'İdarə paneli' : 'Həftənin seçimi'} <ArrowUpRight size={15} /></Link>
        </nav>
      </header>
    </>
  );
}

function Footer() {
  return <footer className="site-footer">
    <div className="page-width footer-main">
      <div className="footer-brand"><Wordmark dark /><p>Seçimlərini gözəlləşdirən kiçik tapıntılar.<br />Gündəlik həyat üçün düşünülmüş məhsullar.</p><div className="footer-social"><span>Gözəl seçimlər</span><span>Hər gün</span></div></div>
      <div className="footer-col"><b>Mercato</b><Link to="/products">Məhsullar</Link><Link to="/products">Kateqoriyalar</Link><Link to="/account">Sifarişlərim</Link></div>
      <div className="footer-col"><b>Yardım</b><span>Bizimlə əlaqə</span><span>Çatdırılma məlumatı</span><span>Tez-tez verilən suallar</span></div>
      <div className="footer-note"><span className="footer-note-icon"><Leaf size={19} /></span><b>Daha düşünülmüş seçimlər.</b><span>Sevdiyin şeylər, daha yaxşı gündəlik.</span></div>
    </div>
    <div className="page-width footer-bottom"><span>© {new Date().getFullYear()} Mercato. Bütün hüquqlar qorunur.</span><span>Şərtlər <i>·</i> Məxfilik</span></div>
  </footer>;
}

function Toast() {
  const { toast } = useStore();
  if (!toast) return null;
  const Icon = toast.kind === 'error' ? CircleHelp : Check;
  return <div className={`toast toast-${toast.kind}`} role="status"><span><Icon size={17} /></span>{toast.message}</div>;
}

function SectionHeading({ eyebrow, title, link, to = '/products' }) {
  return <div className="section-heading"><div><span className="eyebrow">{eyebrow}</span><h2>{title}</h2></div>{link && <Link to={to} className="text-link">{link}<ArrowRight size={16} /></Link>}</div>;
}

function ProductCard({ product, index = 0 }) {
  const { addToCart, wishlist, toggleWishlist, notify } = useStore();
  const navigate = useNavigate();
  const productId = product.id ?? product.productId;
  const isWishlisted = wishlist.some((item) => item.productId === productId);
  const unavailable = Number(product.stock ?? 1) <= 0;

  const add = async (event) => {
    event.stopPropagation();
    try { await addToCart(product); }
    catch (error) { notify(error.message, 'error'); }
  };
  const wish = async (event) => {
    event.stopPropagation();
    try { await toggleWishlist(product); }
    catch (error) { notify(error.message, 'error'); }
  };

  return <article className="product-card" onClick={() => navigate(`/products/${productId}`)}>
    <div className="product-card-visual">
      <Artwork product={{ ...product, id: productId }} />
      <span className="product-tag">{index % 3 === 0 ? 'Seçilmiş' : product.categoryName || 'Mercato'}</span>
      <button className={`wish-button${isWishlisted ? ' is-active' : ''}`} aria-label="Seçilmişlərə əlavə et" onClick={wish}><Heart size={17} fill={isWishlisted ? 'currentColor' : 'none'} /></button>
      <button className="quick-add" onClick={add} disabled={unavailable}>{unavailable ? 'Bitib' : <><Plus size={15} /> Səbətə at</>}</button>
    </div>
    <div className="product-card-meta"><span>{product.categoryName || 'Seçilmiş məhsul'}</span></div>
    <h3>{product.name}</h3>
    <div className="product-card-bottom"><strong>{money(product.price)}</strong><span>{unavailable ? 'Stokda yoxdur' : 'Stokda'}</span></div>
  </article>;
}

function LoadingGrid({ count = 4 }) {
  return <div className="product-grid">{Array.from({ length: count }, (_, index) => <div className="skeleton-card" key={index}><div className="skeleton-art" /><div className="skeleton-line short" /><div className="skeleton-line" /><div className="skeleton-line short" /></div>)}</div>;
}

function EmptyState({ title = 'Hələlik burada heç nə yoxdur', text = 'Bir az sonra yenidən bax.' }) {
  return <div className="empty-state"><span><Package size={28} /></span><h3>{title}</h3><p>{text}</p></div>;
}

function HomePage() {
  const { notify } = useStore();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [prompt, setPrompt] = useState('');
  const [suggestions, setSuggestions] = useState([]);
  const [aiMessage, setAiMessage] = useState('');
  const [aiLoading, setAiLoading] = useState(false);

  useEffect(() => {
    Promise.all([
      request('/Products?pageNumber=1&pageSize=8&sortBy=id&descending=true'),
      request('/Categories'),
    ]).then(([productData, categoryData]) => {
      setProducts(productData?.items || []);
      setCategories(Array.isArray(categoryData) ? categoryData : []);
    }).catch((error) => notify(`Məhsulları yükləmək olmadı: ${error.message}`, 'error'))
      .finally(() => setLoading(false));
  }, [notify]);

  const getIdeas = async (event) => {
    event.preventDefault();
    if (!prompt.trim()) return;
    setAiLoading(true);
    setAiMessage('');
    try {
      const result = await request('/ai/product-recommendations', { method: 'POST', body: { prompt: prompt.trim() } });
      setSuggestions(result?.products || []);
      setAiMessage(result?.message || 'Bunlar sənin axtarışına uyğun seçildi.');
      if (!result?.products?.length) setAiMessage(result?.message || 'Bu sorğuya uyğun məhsul tapılmadı. Başqa cür təsvir etməyə çalış.');
    } catch (error) {
      setAiMessage(error.message || 'Tövsiyələri hazırda yükləmək olmadı.');
    } finally {
      setAiLoading(false);
    }
  };

  const heroProduct = products[0] || { id: 1, name: 'Gündəlik favoritlər', categoryName: 'Mercato seçimləri' };

  return <>
    <section className="hero page-width">
      <div className="hero-copy">
        <span className="eyebrow"><Sparkles size={13} /> HƏR GÜNƏ GÖZƏL BİR TOXUNUŞ</span>
        <h1>Yaxşı şeylər,<br /><em>yaxşı seçilib.</em></h1>
        <p>Sevdiyin gündəlik əşyaları bir az daha maraqlı et. Mercato-da hər seçimdə kiçik bir kəşf var.</p>
        <div className="hero-actions"><Link to="/products" className="button button-dark">Alış-verişə başla <ArrowRight size={16} /></Link><span className="hero-note"><ShieldCheck size={15} /> Rahat və güvənli alış-veriş</span></div>
        <div className="hero-rating"><span className="hero-note"><Package size={15} /> Stokda olan məhsulları indi kəşf et</span></div>
      </div>
      <div className="hero-visual">
        <div className="hero-blob" />
        <div className="hero-orbit hero-orbit-one" />
        <div className="hero-orbit hero-orbit-two" />
        <div className="hero-product"><Artwork product={heroProduct} large /></div>
        <div className="hero-float-card hero-float-top"><span className="float-icon"><Leaf size={16} /></span><div><b>Kiçik kəşflər</b><small>Gözəl gündəlik seçimlər</small></div></div>
        <div className="hero-float-card hero-float-bottom"><span className="float-spark">✳</span><div><b>Özünə bir yaxşılıq et</b><small>Yeni favoritini tap</small></div><ArrowUpRight size={16} /></div>
        <span className="hero-vertical">THE MERCATO EDIT · 01</span>
      </div>
      <div className="hero-counter"><span>01</span><i /> <span>03</span></div>
    </section>

    <section className="promise-strip"><div className="page-width promise-grid"><div><Package size={18} /><span><b>Stokda olan məhsullar</b><small>Mövcudluğu hər zaman göstərilir</small></span></div><div><BadgeCheck size={18} /><span><b>Diqqətlə seçilmiş</b><small>Mercato məhsul kolleksiyası</small></span></div><div><LockKeyhole size={18} /><span><b>Qorunan hesab</b><small>Hesab məlumatların qorunur</small></span></div><div><Heart size={18} /><span><b>Seçimlərini saxla</b><small>Favoritlərini bir yerə yığ</small></span></div></div></section>

    <section className="page-width category-section">
      <SectionHeading eyebrow="NƏ AXTARIRSAN?" title="Bir az gəzişək" link="Hamısına bax" />
      {categories.length ? <div className="category-grid">{categories.slice(0, 6).map((category, index) => {
        const Icon = categoryArt(category.name);
        return <Link key={category.id} to={`/products?categoryId=${category.id}`} className={`category-tile tile-${categoryTones[index % categoryTones.length]}`}><span className="category-icon"><Icon size={24} strokeWidth={1.4} /></span><span className="category-name">{category.name}</span><span className="category-arrow"><ArrowUpRight size={16} /></span></Link>;
      })}</div> : <div className="category-empty"><span>Yeni seçimlər</span><span>Gündəlik favoritlər</span><span>Ev üçün</span><span>Özünə hədiyyə</span></div>}
    </section>

    <section className="page-width featured-section">
      <SectionHeading eyebrow="SƏNİN ÜÇÜN SEÇDİK" title="Bu həftənin favoritləri" link="Bütün məhsullar" />
      {loading ? <LoadingGrid /> : products.length ? <div className="product-grid">{products.slice(0, 4).map((product, index) => <ProductCard product={product} index={index} key={product.id} />)}</div> : <EmptyState title="Məhsullar hələ əlavə olunmayıb" text="Məhsullar görünən kimi bu bölmədə seçilmişləri tapa biləcəksən." />}
    </section>

    <section className="ai-section page-width">
      <div className="ai-card">
        <div className="ai-copy"><span className="eyebrow"><Sparkles size={13} /> AĞILLI SEÇİM</span><h2>Axtardığını sözlə<br />təsvir et.</h2><p>Nə axtardığını yaz, Mercato sənin üçün uyğun məhsulları tapsın.</p><div className="ai-chips"><button onClick={() => setPrompt('Ev üçün rahat və minimalist bir şey')}>Ev üçün rahat bir şey</button><button onClick={() => setPrompt('Hədiyyə üçün maraqlı texnoloji aksesuar')}>Hədiyyə üçün ideya</button></div></div>
        <form className="ai-form" onSubmit={getIdeas}><label htmlFor="ai-prompt">Nə axtarırsan?</label><div className="ai-input-wrap"><input id="ai-prompt" value={prompt} onChange={(event) => setPrompt(event.target.value)} placeholder="Məsələn: iş masam üçün rahat aksesuar..." /><button type="submit" disabled={aiLoading}>{aiLoading ? <span className="spinner" /> : <Send size={17} />}</button></div><small><Sparkles size={12} /> Təbii dildə yaza bilərsən — biz seçim edək.</small>{aiMessage && <p className="ai-message">{aiMessage}</p>}{suggestions.length > 0 && <div className="ai-results">{suggestions.slice(0, 3).map((product, index) => <ProductCard key={product.productId} product={product} index={index + 1} />)}</div>}</form>
      </div>
    </section>

    <section className="page-width editorial-banner"><div><span className="eyebrow">GÜNDƏLİK HƏYAT ÜÇÜN</span><h2>Kiçik seçimlər.<br /><em>Böyük fərq.</em></h2><Link to="/products" className="button button-light">Mercato-nu kəşf et <ArrowRight size={16} /></Link></div><div className="editorial-shape"><span className="editorial-leaf"><Leaf size={74} strokeWidth={0.9} /></span><span className="editorial-circle" /><span className="editorial-label">YAXŞI SEÇİM<br />YAXŞI HİSS</span></div></section>
  </>;
}

function ProductGrid({ products, loading }) {
  if (loading) return <LoadingGrid count={8} />;
  if (!products.length) return <EmptyState title="Bu axtarışda məhsul tapılmadı" text="Axtarış sözünü və ya seçdiyin filtri dəyişməyə çalış." />;
  return <div className="product-grid">{products.map((product, index) => <ProductCard key={product.id} product={product} index={index} />)}</div>;
}

function CatalogPage() {
  const [params, setParams] = useSearchParams();
  const { notify } = useStore();
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const requestedPage = Number(params.get('page') || 1);
  const page = Number.isInteger(requestedPage) && requestedPage > 0 ? requestedPage : 1;
  const search = params.get('search') || '';
  const categoryId = params.get('categoryId') || '';
  const sortBy = params.get('sortBy') || 'id';
  const descending = params.has('descending') ? params.get('descending') === 'true' : true;
  const [resultPage, setResultPage] = useState({ totalCount: 0, totalPages: 0 });

  useEffect(() => { request('/Categories').then((value) => setCategories(Array.isArray(value) ? value : [])).catch(() => {}); }, []);
  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    const query = new URLSearchParams({ pageNumber: String(page), pageSize: '12', sortBy, descending: String(descending) });
    if (search) query.set('search', search);
    if (categoryId) query.set('categoryId', categoryId);
    request(`/Products?${query}`).then((result) => {
      if (cancelled) return;
      setProducts(result?.items || []);
      setResultPage({ totalCount: result?.totalCount || 0, totalPages: result?.totalPages || 0 });
      const effectivePage = Number(result?.pageNumber) || 1;
      if (effectivePage !== page) {
        setParams((current) => {
          const next = new URLSearchParams(current);
          if (effectivePage === 1) next.delete('page');
          else next.set('page', String(effectivePage));
          return next;
        }, { replace: true });
      }
    }).catch((error) => {
      if (!cancelled) notify(error.message, 'error');
    }).finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [page, search, categoryId, sortBy, descending, notify, setParams]);

  const title = categoryId ? categories.find((category) => String(category.id) === categoryId)?.name || 'Kateqoriya' : search ? `“${search}” üçün nəticələr` : sortBy === 'popular' ? 'Trenddə olan məhsullar' : 'Bütün məhsullar';
  const update = (key, value) => {
    const next = new URLSearchParams(params);
    if (!value) next.delete(key); else next.set(key, value);
    if (key !== 'page') next.delete('page');
    setParams(next);
  };
  const updateSort = (sort) => {
    const next = new URLSearchParams(params);
    next.set('sortBy', sort[0]);
    next.set('descending', sort[1]);
    next.delete('page');
    setParams(next);
  };

  return <main className="page-width catalog-page">
    <div className="breadcrumbs"><Link to="/">Ana səhifə</Link><ChevronRight size={14} /><span>Alış-veriş</span></div>
    <div className="catalog-title-row"><div><span className="eyebrow">MERCATO SEÇİMLƏRİ</span><h1>{title}</h1><p>{resultPage.totalCount} seçilmiş məhsul</p></div><label className="sort-select"><span>Sırala:</span><select value={sortBy === 'popular' ? 'popular' : sortBy === 'price' ? (descending ? 'price-desc' : 'price-asc') : sortBy === 'name' ? 'name' : 'newest'} onChange={(event) => {
      const values = { newest: ['id', 'true'], popular: ['popular', 'true'], 'price-asc': ['price', 'false'], 'price-desc': ['price', 'true'], name: ['name', 'false'] };
      updateSort(values[event.target.value]);
    }}><option value="newest">Ən yenilər</option><option value="popular">Ən çox satılan</option><option value="price-asc">Qiymət: aşağıdan yuxarı</option><option value="price-desc">Qiymət: yuxarıdan aşağı</option><option value="name">Ada görə</option></select><ChevronDown size={15} /></label></div>
    <div className="catalog-layout"><aside className="catalog-sidebar"><div className="filter-heading"><b>Kateqoriyalar</b><button onClick={() => setParams(new URLSearchParams())}>Təmizlə</button></div><button className={`filter-option${!categoryId ? ' selected' : ''}`} onClick={() => update('categoryId', '')}><span>Bütün məhsullar</span><ArrowRight size={14} /></button>{categories.map((category) => <button key={category.id} className={`filter-option${categoryId === String(category.id) ? ' selected' : ''}`} onClick={() => update('categoryId', String(category.id))}><span>{category.name}</span><ArrowRight size={14} /></button>)}<div className="sidebar-note"><CircleHelp size={16} /><span>Seçim etməkdə kömək lazımdır? <Link to="/">Ağıllı tövsiyəni sın</Link></span></div></aside>
      <section className="catalog-results"><ProductGrid products={products} loading={loading} />{resultPage.totalPages > 1 && <div className="pagination"><button disabled={page <= 1} onClick={() => update('page', String(page - 1))}><ChevronLeft size={17} /> Əvvəlki</button><span>Səhifə <b>{page}</b> / {resultPage.totalPages}</span><button disabled={page >= resultPage.totalPages} onClick={() => update('page', String(page + 1))}>Növbəti <ChevronRight size={17} /></button></div>}</section></div>
  </main>;
}

function ProductPage() {
  const { id } = useParams();
  const { addToCart, token, notify } = useStore();
  const [product, setProduct] = useState(null);
  const [loadError, setLoadError] = useState('');
  const [reviews, setReviews] = useState(null);
  const [quantity, setQuantity] = useState(1);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const [reviewBusy, setReviewBusy] = useState(false);

  useEffect(() => {
    setProduct(null);
    setLoadError('');
    Promise.all([request(`/Products/${id}`), request(`/ProductReviews/product/${id}`)])
      .then(([productData, reviewData]) => { setProduct(productData); setReviews(reviewData); })
      .catch((error) => { setLoadError(error.message); notify(error.message, 'error'); });
  }, [id, notify]);

  if (!product && loadError) return <main className="page-width not-found"><span className="eyebrow">MƏHSUL TAPILMADI</span><h1>{loadError}</h1><p>Axtarışa qayıdıb başqa məhsul seçə bilərsən.</p><Link to="/products" className="button button-dark">Məhsullara bax <ArrowRight size={15} /></Link></main>;
  if (!product) return <main className="page-width detail-loading"><span className="spinner" /> Məhsul yüklənir...</main>;
  const stock = Number(product.stock || 0);
  const submitReview = async (event) => {
    event.preventDefault();
    setReviewBusy(true);
    try {
      await request('/ProductReviews', { method: 'POST', body: { productId: product.id, rating, comment: comment.trim() || null } });
      const data = await request(`/ProductReviews/product/${id}`);
      setReviews(data);
      setComment('');
      notify('Rəyin əlavə olundu.');
    } catch (error) { notify(error.message, 'error'); }
    finally { setReviewBusy(false); }
  };
  const add = async () => {
    try { await addToCart(product, quantity); }
    catch (error) { notify(error.message, 'error'); }
  };

  return <main className="page-width product-detail-page">
    <div className="breadcrumbs"><Link to="/">Ana səhifə</Link><ChevronRight size={14} /><Link to="/products">Məhsullar</Link><ChevronRight size={14} /><span>{product.name}</span></div>
    <div className="product-detail"><div className="detail-art-wrap"><Artwork product={product} large /><span className="detail-edition">MERCATO SEÇİMİ</span></div><div className="detail-info"><span className="eyebrow">{product.categoryName || 'SEÇİLMİŞ MƏHSUL'}</span><h1>{product.name}</h1><div className="detail-rating"><Stars value={reviews?.averageRating || 0} /><span>{reviews?.averageRating ? Number(reviews.averageRating).toFixed(1) : '—'}</span><a href="#reviews">({reviews?.reviewCount || 0} rəy)</a></div><strong className="detail-price">{money(product.price)}</strong><p className="detail-description">{product.description || 'Gündəlik həyatına yaraşan, diqqətlə seçilmiş bir məhsul. İndi səbətinə əlavə et və öz Mercato seçimini et.'}</p><div className={`stock-note${stock <= 0 ? ' out' : ''}`}><span />{stock > 0 ? `${stock} ədəd stokda mövcuddur` : 'Hazırda stokda yoxdur'}</div><div className="detail-buy-row"><div className="quantity-control"><button onClick={() => setQuantity(Math.max(1, quantity - 1))} aria-label="Azalt"><Minus size={15} /></button><span>{quantity}</span><button onClick={() => setQuantity(Math.min(stock || 1, quantity + 1))} aria-label="Artır"><Plus size={15} /></button></div><button className="button button-dark detail-buy" disabled={stock <= 0} onClick={add}><ShoppingBag size={17} /> Səbətə əlavə et <span>{money(product.price * quantity)}</span></button></div><div className="detail-perks"><span><Truck size={16} /> Sifariş məlumatları hesabında görünür</span><span><ShieldCheck size={16} /> Hesabın qorunur</span><span><Clock3 size={16} /> Sifarişlərini hesabında izlə</span></div><div className="detail-category"><span>Məhsul kodu</span><b>MC-{String(product.id).padStart(4, '0')}</b></div></div></div>
    <section className="reviews-section" id="reviews"><div className="reviews-heading"><div><span className="eyebrow">İSTİFADƏÇİ RƏYLƏRİ</span><h2>Seçimini edənlər nə deyir?</h2></div><div className="reviews-average"><strong>{reviews?.averageRating ? Number(reviews.averageRating).toFixed(1) : '—'}</strong><Stars value={reviews?.averageRating || 0} /><small>{reviews?.reviewCount || 0} rəy</small></div></div><div className="reviews-layout"><div className="reviews-list">{reviews?.reviews?.length ? reviews.reviews.map((review) => <article className="review-card" key={review.reviewId}><div className="review-avatar">{String(review.userId || 'M')[0].toUpperCase()}</div><div><div className="review-user"><b>Mercato alıcısı</b><span>{new Date(review.createdAtUtc).toLocaleDateString('az-AZ')}</span></div><div className="rating-stars">{'★'.repeat(Math.max(0, Number(review.rating) || 0))}{'☆'.repeat(Math.max(0, 5 - (Number(review.rating) || 0)))}</div><p>{review.comment || 'Bu məhsul barədə əlavə qeyd yoxdur.'}</p></div></article>) : <EmptyState title="Hələ rəy yoxdur" text="Bu məhsulu ilk qiymətləndirən sən ol." />}</div>{token ? <form className="review-form" onSubmit={submitReview}><b>Rəyini paylaş</b><small className="review-note">Yalnız bu məhsulu almış müştərilər rəy yaza bilər.</small><label>Qiymətləndirmən</label><div className="rating-picker">{[1, 2, 3, 4, 5].map((value) => <button type="button" key={value} onClick={() => setRating(value)} aria-label={`${value} ulduz`}><Star size={21} fill={value <= rating ? 'currentColor' : 'none'} /></button>)}</div><label htmlFor="review-comment">Qısa rəy</label><textarea id="review-comment" value={comment} onChange={(event) => setComment(event.target.value)} placeholder="Təcrübəni bölüş..." rows="4" /><button className="button button-dark" disabled={reviewBusy}>{reviewBusy ? 'Göndərilir...' : 'Rəy əlavə et'} <ArrowRight size={15} /></button></form> : <div className="review-login"><span><LogIn size={17} /></span><b>Rəy yazmaq istəyirsən?</b><p>Rəyini paylaşmaq üçün hesabına daxil ol.</p><Link to="/login" className="text-link">Daxil ol <ArrowRight size={15} /></Link></div>}</div></section>
  </main>;
}

function CartPage() {
  const { token, email, cart, cartTotal, updateQuantity, removeFromCart, refreshCart, notify } = useStore();
  const navigate = useNavigate();
  const [coupon, setCoupon] = useState('');
  const [busy, setBusy] = useState(false);

  useEffect(() => { if (token) refreshCart().catch((error) => notify(error.message, 'error')); }, [token, refreshCart, notify]);

  const checkout = async () => {
    if (!token) {
      navigate('/login', { state: { from: '/cart' } });
      return;
    }
    if (!cart.length) return;
    setBusy(true);
    let createdOrder = null;
    try {
      if (paymentProvider === 'Stripe') {
        const availability = await request('/Payments/stripe/availability');
        if (!availability?.ready) throw new Error(availability?.message || 'Stripe Checkout hazır deyil.');
      }
      const order = await request('/Orders', { method: 'POST', body: { couponCode: coupon.trim() || null } });
      createdOrder = order;
      await refreshCart();
      const payment = await request(`/Payments/create/${order.orderId}?provider=${paymentProvider}`, { method: 'POST' });
      if (paymentProvider === 'Mock' && String(payment?.checkoutUrl || '').startsWith('mock://')) {
        await request(`/Payments/mock/success/${order.orderId}`, { method: 'POST' });
        await refreshCart();
        navigate(`/order-complete/${order.orderId}`, { state: { order } });
      } else if (payment?.checkoutUrl) {
        window.location.assign(payment.checkoutUrl);
      } else {
        throw new Error('Stripe Checkout səhifəsinin ünvanı alınmadı.');
      }
    } catch (error) {
      if (createdOrder?.orderId) {
        navigate(`/order-complete/${createdOrder.orderId}?payment=retry`, { state: { order: createdOrder } });
        notify(`Sifariş yaradıldı, amma ödəniş səhifəsi açılmadı: ${error.message}`, 'error');
      } else {
        notify(error.message, 'error');
      }
    }
    finally { setBusy(false); }
  };

  if (!cart.length) return <main className="page-width cart-page"><div className="breadcrumbs"><Link to="/">Ana səhifə</Link><ChevronRight size={14} /><span>Səbət</span></div><div className="empty-cart"><span className="empty-cart-icon"><ShoppingBag size={30} /></span><span className="eyebrow">SƏBƏTİN SƏNİ GÖZLƏYİR</span><h1>Hələ heç nə seçməmisən.</h1><p>Mercato-da bir az gəz, bəyəndiklərini səbətə əlavə et.</p><Link to="/products" className="button button-dark">Məhsullara bax <ArrowRight size={16} /></Link></div></main>;

  return <main className="page-width cart-page"><div className="breadcrumbs"><Link to="/">Ana səhifə</Link><ChevronRight size={14} /><span>Səbət</span></div><div className="cart-title"><div><span className="eyebrow">SEÇİMLƏRİN BURADADIR</span><h1>Sənin səbətin <small>({cart.length} məhsul)</small></h1></div><Link to="/products" className="text-link">Alış-verişə davam et <ArrowRight size={15} /></Link></div><div className="cart-layout"><section className="cart-items"><div className="cart-table-head"><span>Məhsul</span><span>Qiymət</span><span>Miqdar</span><span>Cəmi</span></div>{cart.map((item) => <article className="cart-item" key={item.cartItemId || item.productId}><Link to={`/products/${item.productId}`} className="cart-item-art"><Artwork product={{ ...item, id: item.productId }} /></Link><div className="cart-item-title"><Link to={`/products/${item.productId}`}>{item.productName}</Link><span>{item.categoryName || 'Mercato seçimi'}</span><button onClick={() => removeFromCart(item)} className="remove-mobile"><Trash2 size={14} /> Sil</button></div><span className="cart-item-price">{money(item.unitPrice)}</span><div className="quantity-control cart-quantity"><button onClick={() => updateQuantity(item, item.quantity - 1)} disabled={item.quantity <= 1} aria-label="Azalt"><Minus size={14} /></button><span>{item.quantity}</span><button onClick={() => updateQuantity(item, item.quantity + 1)} aria-label="Artır"><Plus size={14} /></button></div><div className="cart-item-total"><b>{money(item.totalPrice || item.unitPrice * item.quantity)}</b><button onClick={() => removeFromCart(item)} aria-label="Məhsulu sil"><Trash2 size={15} /></button></div></article>)}<div className="cart-support-note"><span><ShieldCheck size={17} /></span><div><b>Rahat alış-veriş</b><small>Sifarişin emal zamanı məlumatların qorunur.</small></div></div></section><aside className="order-summary"><span className="eyebrow">SİFARİŞİN</span><h2>Yekun</h2><div className="summary-row"><span>Ara cəm</span><b>{money(cartTotal)}</b></div><label className="coupon-label" htmlFor="coupon">Kupon kodun var?</label><div className="coupon-input"><Tag size={16} /><input id="coupon" value={coupon} onChange={(event) => setCoupon(event.target.value)} placeholder="Kodu daxil et" /></div><div className="summary-total"><span>Ümumi məbləğ</span><b>{money(cartTotal)}</b></div><button className="button button-dark checkout-button" onClick={checkout} disabled={busy}>{busy ? <><span className="spinner" /> Sifariş hazırlanır...</> : <>Sifarişi tamamla <ArrowRight size={16} /></>}</button><p className="checkout-login-note">{token ? <>Hesab: <b>{email}</b></> : <>Davam etmək üçün <Link to="/login" state={{ from: '/cart' }}>daxil ol</Link> və ya qeydiyyatdan keç.</>}</p><div className="checkout-security"><CreditCard size={14} /> {paymentProvider === 'Mock' ? 'Demo ödənişi — kartdan pul çıxılmır' : 'Təhlükəsiz ödəniş Stripe Checkout vasitəsilə'}</div></aside></div></main>;
}

function AuthPage() {
  const [mode, setMode] = useState('login');
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const location = useLocation();
  const { token, signIn, notify } = useStore();
  const destination = location.state?.from || '/account';

  if (token && mode !== 'forgot') return <Navigate to={destination} replace />;

  const submit = async (event) => {
    event.preventDefault();
    setBusy(true); setError(''); setMessage('');
    try {
      if (mode === 'register') {
        const result = await request('/Auth/register', { method: 'POST', body: { fullName: fullName.trim(), email: email.trim(), password } });
        setMode('login');
        setMessage(result?.message || 'Qeydiyyat tamamlandı. Email təsdiqləmə linkini yoxla.');
      } else if (mode === 'forgot') {
        const result = await request('/Auth/forgot-password', { method: 'POST', body: { email: email.trim() } });
        setMessage(result?.message || 'Əgər bu email sistemdə varsa, şifrə yeniləmə linki göndərildi.');
      } else {
        await signIn(email.trim(), password);
        notify('Xoş gəldin!');
        navigate(destination, { replace: true });
      }
    } catch (err) { setError(err.message); }
    finally { setBusy(false); }
  };

  return <main className="auth-page page-width"><div className="auth-art"><div className="auth-art-copy"><span className="eyebrow">YAXŞI SEÇİMLƏR SƏNİ GÖZLƏYİR</span><h2>Mercato-da<br /><em>öz yerin var.</em></h2><p>Seçimlərini yadda saxla, sifarişlərini izləyib yeniliklərdən xəbər tut.</p><span className="auth-art-seal"><Heart size={17} /> Yaxşı seçilib.</span></div><div className="auth-art-sphere" /><div className="auth-art-object"><ShoppingBag size={88} strokeWidth={0.8} /></div><span className="auth-art-side">THE MERCATO COMMUNITY · 01</span></div><div className="auth-form-panel"><div className="auth-mobile-logo"><Wordmark /></div><span className="eyebrow">{mode === 'register' ? 'YENİ HESAB' : mode === 'forgot' ? 'ŞİFRƏNİ YENİLƏ' : 'XİDMƏTİMİZƏ XOŞ GƏLDİN'}</span><h1>{mode === 'register' ? 'Bizə qoşul.' : mode === 'forgot' ? 'Narahat olma.' : 'Yenidən xoş gəldin.'}</h1><p className="auth-subtitle">{mode === 'register' ? 'Bir neçə addımda Mercato hesabını yarat.' : mode === 'forgot' ? 'Email ünvanını yaz, şifrə yeniləmə linki göndərək.' : 'Seçimlərinə davam etmək üçün hesabına daxil ol.'}</p>{message && <div className="form-message success-message"><Check size={16} />{message}</div>}{error && <div className="form-message error-message"><CircleHelp size={16} />{error}</div>}<form className="auth-form" onSubmit={submit}>{mode === 'register' && <label>Ad və soyad<input value={fullName} onChange={(event) => setFullName(event.target.value)} autoComplete="name" placeholder="Ad Soyad" required /></label>}<label>Email ünvanı<input value={email} onChange={(event) => setEmail(event.target.value)} type="email" autoComplete="email" placeholder="sən@email.com" required /></label>{mode !== 'forgot' && <label>Şifrə<input value={password} onChange={(event) => setPassword(event.target.value)} type="password" autoComplete={mode === 'register' ? 'new-password' : 'current-password'} placeholder={mode === 'register' ? 'Ən azı 6 simvol' : 'Şifrən'} minLength={6} required /></label>}{mode === 'login' && <button type="button" className="forgot-link" onClick={() => { setMode('forgot'); setMessage(''); setError(''); }}>Şifrəni unutdun?</button>}<button className="button button-dark auth-submit" disabled={busy}>{busy ? 'Bir az gözlə...' : mode === 'register' ? 'Hesab yarat' : mode === 'forgot' ? 'Link göndər' : <>Daxil ol <ArrowRight size={16} /></>}</button></form><div className="auth-switch">{mode === 'register' ? <>Artıq hesabın var? <button onClick={() => { setMode('login'); setMessage(''); setError(''); }}>Daxil ol</button></> : mode === 'forgot' ? <button onClick={() => { setMode('login'); setMessage(''); setError(''); }}>Daxil ol səhifəsinə qayıt</button> : <>Hesabın yoxdur? <button onClick={() => { setMode('register'); setMessage(''); setError(''); }}>Qeydiyyatdan keç</button></>}</div><div className="auth-assurance"><LockKeyhole size={15} /> Şəxsi məlumatların qorunur.</div></div></main>;
}

function AccountPage() {
  const { token, email, signOut, notify, isAdmin } = useStore();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => {
    if (!token) { setLoading(false); return; }
    request('/Orders').then((value) => setOrders(Array.isArray(value) ? value : [])).catch((error) => notify(error.message, 'error')).finally(() => setLoading(false));
  }, [token, notify]);
  if (!token) return <Navigate to="/login" state={{ from: '/account' }} replace />;
  return <main className="page-width account-page"><div className="breadcrumbs"><Link to="/">Ana səhifə</Link><ChevronRight size={14} /><span>Hesabım</span></div><div className="account-heading"><div className="account-avatar">{email.slice(0, 1).toUpperCase() || 'M'}</div><div><span className="eyebrow">SƏNİN MERCATO-N</span><h1>Xoş gəldin!</h1><p>{email}</p></div>{isAdmin && <Link to="/admin" className="button button-outline">İdarə paneli <ArrowRight size={15} /></Link>}<button className="button button-outline" onClick={signOut}>Hesabdan çıx <ArrowRight size={15} /></button></div><section className="orders-section"><div className="section-heading"><div><span className="eyebrow">SİFARİŞLƏRİN</span><h2>Sifariş tarixçəsi</h2></div><Link to="/wishlist" className="text-link">Seçilmişlər <Heart size={15} /></Link></div>{loading ? <div className="detail-loading"><span className="spinner" /> Sifarişlər yüklənir...</div> : orders.length ? <div className="orders-list">{orders.map((order) => <article className="order-card" key={order.id}><div className="order-card-top"><div><span>SİFARİŞ № {String(order.id).padStart(5, '0')}</span><b>{new Date(order.createdAtUtc).toLocaleDateString('az-AZ', { day: 'numeric', month: 'long', year: 'numeric' })}</b></div><span className={`order-status status-${String(order.status).toLowerCase()}`}>{order.status}</span></div><div className="order-card-bottom"><div>{order.items?.map((item) => <span key={`${order.id}-${item.productId}`}>{item.quantity} × {item.productName}</span>)}</div><b>{money(order.totalPrice)}</b></div></article>)}</div> : <div className="account-empty"><span><Package size={25} /></span><h3>İlk sifarişin hələ qabaqdadır.</h3><p>Mercato seçimlərinə bax və səbətini yarat.</p><Link to="/products" className="button button-dark">Məhsullara bax <ArrowRight size={15} /></Link></div>}</section></main>;
}

function WishlistPage() {
  const { token, wishlist, refreshWishlist, notify } = useStore();
  useEffect(() => { if (token) refreshWishlist().catch((error) => notify(error.message, 'error')); }, [token, refreshWishlist, notify]);
  if (!token) return <main className="page-width wishlist-page"><div className="breadcrumbs"><Link to="/">Ana səhifə</Link><ChevronRight size={14} /><span>Seçilmişlər</span></div><div className="empty-cart"><span className="empty-cart-icon"><Heart size={28} /></span><span className="eyebrow">SEÇİMLƏRİN SƏNİN ÜÇÜN</span><h1>Sevdiklərini bir yerə yığ.</h1><p>Seçilmişlər siyahını görmək üçün hesabına daxil ol.</p><Link to="/login" state={{ from: '/wishlist' }} className="button button-dark">Daxil ol <ArrowRight size={16} /></Link></div></main>;
  return <main className="page-width wishlist-page"><div className="breadcrumbs"><Link to="/">Ana səhifə</Link><ChevronRight size={14} /><span>Seçilmişlər</span></div><div className="catalog-title-row"><div><span className="eyebrow">SƏNİN SEÇİMLƏRİN</span><h1>Seçilmişlər</h1><p>{wishlist.length} məhsul</p></div></div>{wishlist.length ? <div className="product-grid">{wishlist.map((product, index) => <ProductCard key={product.productId} product={{ ...product, id: product.productId, name: product.productName }} index={index} />)}</div> : <EmptyState title="Seçilmişlər siyahın boşdur" text="Məhsulun üzərindəki ürəyi seçərək favoritlərini yadda saxla." />}</main>;
}

function OrderCompletePage() {
  const { id } = useParams();
  const location = useLocation();
  const { notify } = useStore();
  const [payment, setPayment] = useState(null);
  const [checking, setChecking] = useState(true);
  const [retryBusy, setRetryBusy] = useState(false);
  const [retryError, setRetryError] = useState('');
  const [status, setStatus] = useState('checking');
  const cancelled = new URLSearchParams(location.search).get('payment') === 'cancelled';

  useEffect(() => {
    let disposed = false;
    let timer;
    let attempts = 0;
    const checkPayment = async () => {
      try {
        const result = await request(`/Payments/order/${id}`);
        if (disposed) return;
        setPayment(result);
        const paymentStatus = typeof result?.status === 'string'
          ? result.status.toLowerCase()
          : Number(result?.status) === 2 ? 'succeeded' : [3, 4].includes(Number(result?.status)) ? 'failed' : 'pending';
        if (paymentStatus === 'succeeded') {
          setStatus('succeeded');
          setChecking(false);
        } else if (paymentStatus === 'failed' || paymentStatus === 'cancelled') {
          setStatus('failed');
          setChecking(false);
        } else if (attempts++ < 6) {
          timer = window.setTimeout(checkPayment, 2000);
        } else {
          setStatus('pending');
          setChecking(false);
        }
      } catch {
        if (disposed) return;
        if (attempts++ < 2) timer = window.setTimeout(checkPayment, 1500);
        else { setStatus('pending'); setChecking(false); }
      }
    };
    checkPayment();
    return () => { disposed = true; window.clearTimeout(timer); };
  }, [id]);

  const resumePayment = async () => {
    setRetryBusy(true);
    setRetryError('');
    try {
      if (payment?.checkoutUrl?.startsWith('https://checkout.stripe.com/')) {
        window.location.assign(payment.checkoutUrl);
        return;
      }
      const availability = await request('/Payments/stripe/availability');
      if (!availability?.ready) throw new Error(availability?.message || 'Stripe Checkout hazır deyil.');
      const result = await request(`/Payments/create/${id}?provider=Stripe`, { method: 'POST' });
      if (!result?.checkoutUrl) throw new Error('Stripe Checkout səhifəsinin ünvanı alınmadı.');
      window.location.assign(result.checkoutUrl);
    } catch (error) {
      setRetryError(error.message);
      notify(error.message, 'error');
    } finally { setRetryBusy(false); }
  };

  const paid = status === 'succeeded';
  const failed = status === 'failed';
  return <main className="page-width order-complete"><span className="complete-check"><Check size={34} /></span><span className="eyebrow">{paid ? 'ÖDƏNİŞ TƏSDİQLƏNDİ' : failed ? 'ÖDƏNİŞ TAMAMLANMADI' : 'ÖDƏNİŞ GÖZLƏNİLİR'}</span><h1>{paid ? 'Yaxşı seçim etdin.' : failed ? 'Ödəniş tamamlanmadı.' : checking ? 'Ödəniş yoxlanılır.' : 'Ödəniş təsdiqini gözləyir.'}</h1><p>{paid ? `Sifarişin #${String(id).padStart(5, '0')} ödənildi. Ətraflı məlumat hesabındakı sifariş tarixçəsindədir.` : cancelled ? 'Stripe Checkout-dan ödənişi tamamlamadan qayıtdın. Sifarişin saxlanılıb, istəsən ödənişi yenidən davam etdirə bilərsən.' : checking ? 'Stripe-dan təsdiq cavabı gözləyirik. Bu adətən bir neçə saniyə çəkir.' : `Sifarişin #${String(id).padStart(5, '0')} yaradılıb, amma ödəniş hələ təsdiqlənməyib.`}</p>{retryError && <div className="form-message error-message">{retryError}</div>}<div className="complete-actions">{!paid && paymentProvider === 'Stripe' && <button className="button button-dark" onClick={resumePayment} disabled={retryBusy}>{retryBusy ? 'Açılır...' : payment?.checkoutUrl ? 'Ödənişi davam et' : 'Stripe ilə yenidən yoxla'} <ArrowRight size={16} /></button>}<Link to="/account" className="button button-outline">Sifarişlərimə bax <ArrowRight size={16} /></Link><Link to="/products" className="button button-outline">Alış-verişə davam et</Link></div><div className="complete-note"><Truck size={17} />{paid ? 'Sifarişin hazırlananda sənə xəbər verəcəyik.' : 'Ödəniş statusunu hesabından da görə bilərsən.'}</div></main>;
}

function App() {
  const location = useLocation();
  const { toast } = useStore();
  useEffect(() => { window.scrollTo({ top: 0, behavior: 'smooth' }); }, [location.pathname]);
  return <div className="app-shell"><Header /><Routes><Route path="/" element={<HomePage />} /><Route path="/products" element={<CatalogPage />} /><Route path="/products/:id" element={<ProductPage />} /><Route path="/cart" element={<CartPage />} /><Route path="/login" element={<AuthPage />} /><Route path="/account" element={<AccountPage />} /><Route path="/admin/*" element={<AdminPage />} /><Route path="/wishlist" element={<WishlistPage />} /><Route path="/order-complete/:id" element={<OrderCompletePage />} /><Route path="*" element={<main className="page-width not-found"><span className="eyebrow">SƏHİFƏ TAPILMADI</span><h1>Bu yol bir az itib.</h1><p>Amma Mercato-da tapmağa dəyər çox şey var.</p><Link to="/" className="button button-dark">Ana səhifəyə qayıt <ArrowLeft size={16} /></Link></main>} /></Routes><Footer />{toast && <Toast />}</div>;
}

export default App;
